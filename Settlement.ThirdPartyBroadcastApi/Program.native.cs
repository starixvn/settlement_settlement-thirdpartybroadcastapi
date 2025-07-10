using Fanex.Data.SqlServer;
using Settlement.ThirdPartyBroadcastApi;
using Settlement.ThirdPartyBroadcastApi.Middleware;
using ThirdPartyBroadcast.Models;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
services.AddFanexSql(builder.Configuration);
services.Configure<AppSettingsKey>(builder.Configuration);
services.AddBroadcastServices(builder.Configuration, builder.Environment);
services.AddHealthChecks();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseMiddleware<ErrorHandler>();

app.UseRouting();

app.UseCors(DependencyInjection.AllowOrigin);
app.MapHealthChecks("/healthz");

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();