using Fanex.Data.SqlServer;
using Settlement.ThirdPartyBroadcastApi;
using Settlement.ThirdPartyBroadcastApi.Middleware;
using ThirdPartyBroadcast.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFanexSql(builder.Configuration);

builder.Services.Configure<AppSettingsKey>(builder.Configuration);

builder.Services.AddBroadcastServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseHttpsRedirection();

app.UseMiddleware<ErrorHandler>();

app.UseRouting();

app.UseCors(DependencyInjection.AllowOrigin);

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

app.Run();