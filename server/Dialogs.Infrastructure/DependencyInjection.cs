using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using StackExchange.Redis;
using MassTransit;
using Microsoft.Extensions.Configuration;
using System.Text;
using EventBus;
using EventBus.Events;
using Dialogs.Domain.Interfaces;
using Dialogs.Infrastructure.Mapping;
using Dialogs.Infrastructure.Repositories;

namespace Dialogs.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(cfg => 
        {
            cfg.AddProfile<InfrastructureProfile>();
        }, typeof(DependencyInjection).Assembly);

        var redisConnectionString = configuration.GetSection("RedisSettings:ConnectionString").Value;
        services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddScoped<IUserRepository>(sp =>
        {
            var connectionString = configuration.GetSection("DatabaseSettings:ConnectionString").Value;
            var mapper = sp.GetRequiredService<IMapper>();
            return new UserRepository(connectionString, mapper);
        });


        services.AddScoped<IDialogRepository>(sp =>
        {
            var redis = sp.GetRequiredService<IConnectionMultiplexer>();
            return new RedisDialogRepository(redis, databaseIndex: 1);
        });

        return services;
    }
}