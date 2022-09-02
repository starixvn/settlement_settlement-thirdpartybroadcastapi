using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Fanex.Data;
using Fanex.Data.Repository;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.AspNetCore;
using Fanex.Logging.Extensions.Database;
using Fanex.Logging.Sentry;
using Fanex.Settlement.ScaleOut;
using Fanex.Settlement.ScaleOut.Services;
using Kaden.XmlConverter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sentry;
using ThirdPartyBroadcast.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Settlement.ThirdPartyBroadcastApi;
using ThirdPartyBroadcast;
using ThirdPartyBroadcast.Services;

namespace BroadcastApi
{
    public class Startup
    {
        private const string allowOrigin = "AllowOrigin";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureCORS(services);
            ConfigureFanex();
            ConfigureAutoMapper(services);
            ConfigInternalServices(services);
            services.AddMediatR(typeof(ThirdPartyBroadcastController).GetTypeInfo().Assembly);

            services.AddControllers();
        }

        private static void ConfigureCORS(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(allowOrigin,
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
            });
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
            services.AddScoped<LogRequestResponseAttribute>();
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

        private void ConfigureFanex()
        {
            var connections = Configuration.GetSection("ConnectionStrings")
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
                .UseDefaultDbSettingProvider(resourcePath: Configuration.GetSection("StoredProcedurePath").Value)
                .Run();

            LogManager
                .SetDefaultLogCategory(Configuration.GetSection("LogCategory").Value)
                .Use(new List<ILogEngine>
                {
                    new SentryLogging(new SentryEngineOptions
                    {
                        Dsn = new Dsn(Configuration.GetSection("SentryToken").Value)
                    })
                });

            ExceptionHandler
                .UseConfig(new LoggingConfiguration())
                .UseGetDbExceptionInfo(DatabaseInfoGetters.GetDbExceptionInfo);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseMiddleware<ErrorHandler>();

            app.UseRouting();

            app.UseCors(allowOrigin);

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}