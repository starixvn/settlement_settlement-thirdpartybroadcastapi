using AutoMapper;
using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Data.Repository;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.AspNetCore;
using Fanex.Logging.Extensions.Database;
using Fanex.Logging.Sentry.RavenClient;
using Fanex.MailDeliver.MailClient;
using Fanex.Settlement.Converter.Services;
using Fanex.Settlement.ScaleOut;
using Fanex.Settlement.ScaleOut.Services;
using MassTransit;
using ONEbook.UM;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Transport;
using SharpRaven;
using StackExchange.Redis;
using ThirdPartyBroadcast;
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
        StartUmSetting(configuration);
        ConfigureFanex(configuration);
        services.Configure<IConfiguration>(configuration);
        services.ConfigureCors();
        services.ConfigureAutoMapper();
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

    private static void ConfigureFanex(IConfiguration configuration)
    {
        LogManager
            .SetDefaultLogCategory(configuration.GetSection("LogCategory").Value)
            .Use(new List<ILogEngine>
            {
                new SentryLogging(new SentryEngineOptions
                {
                    Dsn = new Dsn(configuration.GetSection("SentryToken").Value)
                })
            });

        ExceptionHandler
            .UseConfig(new LoggingConfiguration())
            .UseGetDbExceptionInfo(DatabaseInfoGetters.GetDbExceptionInfo);

        AppSettings.AppMailId = configuration.GetSection("MailServer:Id").Value;
        AppSettings.AppMailAddress = configuration.GetSection("MailServer:Address").Value;
    }

    private static void ConfigureAutoMapper(this IServiceCollection services)
    {
        var mapperConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(typeof(ScaleOutMappingProfile));
            mc.AddProfile(typeof(ThirdPartyBroadcastMappingProfile));
        });

        var mapper = mapperConfig.CreateMapper();
        services.AddSingleton(mapper);
    }

    private static void ConfigInternalServices(this IServiceCollection services)
    {
        services.AddTransient<ILogger, Logger>();
        services.AddTransient<ScaleOutSettleThirdPartyService, ScaleOutSettleThirdPartyServiceImp>();
        services.AddTransient<IDynamicRepository, DynamicRepository>();
        services.AddTransient<XmlConvertService, XmlConvertServiceImp>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddTransient<NumberGameService, NumberGameServiceImp>();
        services.AddTransient<FastMarketService, FastMarketServiceImp>();
        services.AddTransient<PinGoalService, PinGoalServiceImp>();
        services.AddTransient<ScaleOutSettleService, ScaleOutSettleServiceImp>();
        services.AddTransient<CalculateService, CalculateServiceImp>();
        services.AddTransient<GeneralService, GeneralServiceImp>();
        services.AddTransient<LogDirectlyService, LogDirectlyServiceImp>();
        services.AddTransient<MoveTicketService, MoveTicketServiceImp>();
        services.AddTransient<PublishMessageService, PublishMessageServiceImp>();
        services.AddTransient<FlagService, FlagServiceImp>();
        services.AddTransient<ManageQueueService, ManageQueueServiceImp>();
        services.AddScoped<LogRequestAttribute>();
        services.AddSingleton<WriteLogService, WriteLogServiceImp>();
        services.AddSingleton<IMaintenanceService, MaintenanceServiceImp>();
    }

    private static void ConfigRedisCache(this IServiceCollection service, IConfiguration configuration)
    {
        var redisConfig = configuration.GetSection("AuthSettings:RedisConfig").Value;
        var applicationName = configuration.GetSection("AuthSettings:ChannelPrefix").Value;
        service.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect($"{redisConfig},ChannelPrefix={applicationName}"));
        service.AddSingleton<ICache>(new RedisCache(new RedisCacheOptions
        {
            ConnectionString = redisConfig,
            ApplicationName = applicationName,
        }));
        service.AddSingleton<ICacheService>(new CacheService());
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

    private static void StartUmSetting(IConfiguration configuration)
    {
        var umConfig = new UMClientConfiguration().Initialize(configuration);

        UMClientManager
            .UseConfig(umConfig)
            .UseCacheService(new UMCacheService(new CacheService()))
            .UseRestClient(new UMRestClient(umConfig));
    }
}