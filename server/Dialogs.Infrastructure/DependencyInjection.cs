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
using Dialogs.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

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


        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(configuration.GetSection("DatabaseSettings:ConnectionString").Value));

        services.AddScoped<IUserRepository>(sp =>
        {
            var connectionString = configuration.GetSection("DatabaseSettings:ConnectionString").Value;
            var mapper = sp.GetRequiredService<IMapper>();
            return new UserRepository(connectionString, mapper);
        });


        services.AddScoped<IDialogRepository>(sp =>
        {
            var connectionString = configuration.GetSection("DatabaseSettings:ConnectionString").Value;
            var mapper = sp.GetRequiredService<IMapper>();
            return new PostgresDialogRepository(connectionString, mapper);
        });

        return services;
    }
}