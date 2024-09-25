using System.Text;
using EventBus;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Dialogs.Infrastructure.Services;
using Dialogs.Infrastructure.Configuration;
using Dialogs.Infrastructure.SagaStates;
using Microsoft.EntityFrameworkCore;
using Dialogs.Infrastructure;
using Dialogs.Api.Sagas;
using System.Reflection;
using Marten;
using Dialogs.Api.Consumers;
using Dialogs.Infrastructure.DbContexts;
using Dialogs.Application.Dialogs.Commands.UpdateUnreadMessageCount;
using Dialogs.Application.Dialogs.Commands.MarkDialogMessagesAsRead;
using Dialogs.Application.Dialogs.Commands.ResetUnreadMessageCount;

namespace Dialogs.Api;
public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
        var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        });

        services.AddMarten(options =>
        {
            var connectionString = configuration.GetSection("DatabaseSettings:ConnectionString").Value;
            options.Connection(connectionString);
            options.Schema.For<DialogMessageSagaData>();
        });

        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.AddSagaStateMachine<DialogMessageSaga, DialogMessageSagaData>()
                .EntityFrameworkRepository(repository =>
                {
                    repository.ExistingDbContext<AppDbContext>();
                    repository.UsePostgres();
                });


            busConfigurator.AddConsumer<UpdateUnreadMessageCountCommandHandler>();
            busConfigurator.AddConsumer<MarkDialogMessagesAsReadCommandHandler>();
            busConfigurator.AddConsumer<ResetUnreadMessageCountCommandHandler>();


            busConfigurator.UsingRabbitMq((context, rabbitMqConfigurator) =>
            {
                var rabbitMqSettings = configuration.GetSection("RabbitMqSettings");

                rabbitMqConfigurator.Host(rabbitMqSettings["HostName"], "/", h =>
                {
                    h.Username(rabbitMqSettings["UserName"]);
                    h.Password(rabbitMqSettings["Password"]);
                });

                rabbitMqConfigurator.UseInMemoryOutbox();

                rabbitMqConfigurator.ConfigureEndpoints(context);
            });

        });

        services.AddTransient<IEventBus,RabbitMQEventBus>();

        return services;
    }
}