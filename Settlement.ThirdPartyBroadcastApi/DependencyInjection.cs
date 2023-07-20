using AutoMapper;
using Fanex.Data;
using Fanex.Data.Repository;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
using Fanex.Settlement.ScaleOut;
using Fanex.Settlement.ScaleOut.Services;
using Kaden.XmlConverter;
using MediatR;
using System.Reflection;
using Fanex.Logging.Extensions.AspNetCore;
using Fanex.Logging.Sentry.RavenClient;
using SharpRaven;
using ThirdPartyBroadcast;
using ThirdPartyBroadcast.ActionFilters;
using ThirdPartyBroadcast.Controllers;
using ThirdPartyBroadcast.Services;
using ILogger = Fanex.Logging.ILogger;

namespace Settlement.ThirdPartyBroadcastApi;

public static class DependencyInjection
{
    public const string AllowOrigin = "AllowOrigin";

    public static void AddBroadcastServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IConfiguration>(configuration);

        ConfigureCors(services);
        ConfigureFanex(configuration);
        ConfigureAutoMapper(services);
        ConfigInternalServices(services);

        services.AddControllers();
        services.AddMediatR(typeof(ThirdPartyBroadcastController).GetTypeInfo().Assembly);
    }

    private static void ConfigureCors(IServiceCollection services)
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
        var connections = configuration.GetSection("ConnectionStrings")
            .GetChildren()
            .AsEnumerable()
            .ToDictionary(
                x => x.GetValue<string>("Name"),
                y => new ConnectionConfiguration(y.GetValue<string>("Name"), y.GetValue<string>("ConnectionString"), y.GetValue<string>("ProviderName"))
            );

        DbSettingProviderManager
            .StartNewSession()
            .Use(connections)
            .WithSqlServer()
            .UseDefaultDbSettingProvider(resourcePath: configuration.GetSection("StoredProcedurePath").Value)
            .Run();

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
    }

    private static void ConfigureAutoMapper(IServiceCollection services)
    {
        var mapperConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(typeof(ScaleOutMappingProfile));
            mc.AddProfile(typeof(ThirdPartyBroadcastMappingProfile));
        });

        var mapper = mapperConfig.CreateMapper();
        services.AddSingleton(mapper);
    }

    private static void ConfigInternalServices(IServiceCollection services)
    {
        services.AddTransient<ILogger, Logger>();
        services.AddTransient<ScaleOutSettleThirdPartyService, ScaleOutSettleThirdPartyServiceImp>();
        services.AddTransient<IDynamicRepository, DynamicRepository>();
        services.AddTransient<XmlConvertService, XmlConvertServiceImp>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddTransient<HappyFiveService, HappyFiveServiceImp>();
        services.AddTransient<NumberGameService, NumberGameServiceImp>();
        services.AddTransient<FastMarketService, FastMarketServiceImp>();
        services.AddTransient<BetBuilderService, BetBuilderServiceImp>();
        services.AddTransient<VirtualBetRadarService, VirtualBetRadarServiceImp>();
        services.AddTransient<VirtualSoccerService, VirtualSoccerServiceImp>();
        services.AddTransient<PinGoalService, PinGoalServiceImp>();
        services.AddTransient<BitCoinService, BitCoinServiceImp>();
        services.AddTransient<OutrightService, OutrightServiceImp>();
        services.AddTransient<ScaleOutSettleService, ScaleOutSettleServiceImp>();
        services.AddTransient<CalculateService, CalculateServiceImp>();
        services.AddTransient<GeneralService, GeneralServiceImp>();
        services.AddTransient<LogDirectlyService, LogDirectlyServiceImp>();
        services.AddTransient<LeapGamingService, LeapGamingServiceImp>();

        services.AddScoped<LogRequestAttribute>();
    }
}