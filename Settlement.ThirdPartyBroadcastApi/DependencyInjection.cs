using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Data.Repository;
using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using MassTransit;
using ONEbook.UM.DependencyInjection;
using StackExchange.Redis;
using ThirdPartyBroadcast.ActionFilters;
using ThirdPartyBroadcast.Controllers;
using ThirdPartyBroadcast.Services;
using ILogger = Fanex.Logging.ILogger;

namespace Settlement.ThirdPartyBroadcastApi;

public static class DependencyInjection
{
    public const string AllowOrigin = "AllowOrigin";

    public static void AddBroadcastServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
    {
        services.AddOneBookUmForService(configuration);
        services.AddFanexLogSentry(configuration);
        services.ConfigureCors();
        services.ConfigInternalServices();
        services.ConfigRedisCache(configuration);
        services.ConfigureMassTransit(configuration);
        services.AddControllers();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ThirdPartyBroadcastController).Assembly));
    }

    private static void ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(AllowOrigin,
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
    }

    private static void ConfigInternalServices(this IServiceCollection services)
    {
        services.AddTransient<ILogger, Logger>();
        services.AddTransient<IDynamicRepository, DynamicRepository>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddTransient<PublishMessageService, PublishMessageServiceImp>();
        services.AddTransient<FlagService, FlagServiceImp>();
        services.AddTransient<ManageQueueService, ManageQueueServiceImp>();
        services.AddScoped<LogRequestAttribute>();
        services.AddSingleton<WriteLogService, WriteLogServiceImp>();
    }

    private static void ConfigRedisCache(this IServiceCollection service, IConfiguration configuration)
    {
        var redisConfig = configuration.GetSection("Fanex.Caching.Redis:RedisConfig").Value;
        var applicationName = configuration.GetSection("Fanex.Caching.Redis:ChannelPrefix").Value;
        service.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect($"{redisConfig},ChannelPrefix={applicationName}"));
        service.AddSingleton<ICache>(new RedisCache(new RedisCacheOptions
        {
            ConnectionString = redisConfig,
            ApplicationName = applicationName,
        }));
    }

    private static void ConfigureMassTransit(this IServiceCollection service, IConfiguration configuration)
    {
        var host = configuration.GetSection("RabbitMQ:Hosting").Value;
        var virtualHost = configuration.GetSection("RabbitMQ:VirtualHost").Value;
        var username = configuration.GetSection("RabbitMQ:Username").Value;
        var password = configuration.GetSection("RabbitMQ:Password").Value;

        service.AddMassTransit(busConfigurator =>
        {
            busConfigurator.UsingRabbitMq((_, busFactoryConfigurator) =>
            {
                busFactoryConfigurator.Host(host, virtualHost, h =>
                {
                    h.Username(username);
                    h.Password(password);
                });
            });
        });
    }
}