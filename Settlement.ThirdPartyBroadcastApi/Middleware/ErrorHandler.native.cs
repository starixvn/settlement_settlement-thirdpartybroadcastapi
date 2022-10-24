using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
using ILogger = Fanex.Logging.ILogger;

namespace Settlement.ThirdPartyBroadcastApi;

public class ErrorHandler
{
    private readonly RequestDelegate next;
    private readonly ILogger logger;
    public IConfiguration Configuration { get; }

    public ErrorHandler(RequestDelegate next, ILogger logger, IConfiguration configuration)
    {
        this.next = next;
        this.logger = logger;
        Configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleException(context, exception);
        }
    }

    private async Task HandleException(HttpContext context, Exception exception)
    {
        var logMessage = new LogMessage(exception.Message)
                    .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(exception))
                    .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                    .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(exception))
                    .ToString();
        await logger.ErrorAsync(logMessage);

        var method = context.Request.Method.ToLowerInvariant();
        var requestedWith = context.Request.Headers["X-Requested-With"];

        if ((method == "post" || method == "put" || method == "delete") && requestedWith == "XMLHttpRequest")
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }

        await context.Response.WriteAsync(exception.Message);

        var env = Configuration.GetSection("Environment").Value;

        if (!env.Equals("LIVE", StringComparison.CurrentCultureIgnoreCase))
        {
            await context.Response.WriteAsync(Environment.NewLine);
            await context.Response.WriteAsync(exception.StackTrace);
        }
    }
}