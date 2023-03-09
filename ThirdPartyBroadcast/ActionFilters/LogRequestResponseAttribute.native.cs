using Fanex.Logging;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using ILogger = Fanex.Logging.ILogger;

namespace ThirdPartyBroadcast.ActionFilters;

[AttributeUsage(AttributeTargets.Method)]
public class LogRequestAttribute : ActionFilterAttribute
{
    private readonly ILogger logger;

    private readonly AppSettingsKey appSettingsKey;

    public LogRequestAttribute(IOptions<AppSettingsKey> appSettingsKey)
    {
        this.logger = new Logger();
        this.appSettingsKey = appSettingsKey.Value;
    }

    public override async void OnActionExecuting(ActionExecutingContext context)
    {
        if (appSettingsKey.EnableLogRequest)
        {
            var model = context.ActionArguments;
            var serialString = JsonConvert.SerializeObject(model);
            var url = context.HttpContext.Request.GetDisplayUrl();
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Url: {url}");
            stringBuilder.AppendLine($"Params: {serialString}");

            await logger.InfoAsync(stringBuilder.ToString());
        }

        base.OnActionExecuting(context);
    }
}