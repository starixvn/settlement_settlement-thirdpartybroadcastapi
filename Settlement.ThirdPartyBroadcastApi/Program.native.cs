using Settlement.ThirdPartyBroadcastApi;
using Settlement.ThirdPartyBroadcastApi.Middleware;
using ThirdPartyBroadcast.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettingsKey>(builder.Configuration);

builder.Services.AddBroadcastServices(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();

app.UseMiddleware<ErrorHandler>();

app.UseRouting();

app.UseCors(DependencyInjection.AllowOrigin);

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

#if DEBUG
app.Run();
#else
await app.RunAsync("http://0.0.0.0:5000");
#endif