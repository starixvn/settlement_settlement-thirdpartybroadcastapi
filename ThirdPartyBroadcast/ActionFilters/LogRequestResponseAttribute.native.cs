using Fanex.Logging;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.Text;
using ILogger = Fanex.Logging.ILogger;

namespace ThirdPartyBroadcast.ActionFilters;

[AttributeUsage(AttributeTargets.Method)]
public class LogRequestAttribute(IOptions<AppSettingsKey> appSettingsKey) : ActionFilterAttribute
{
    private readonly ILogger logger = new Logger();


    public override async void OnActionExecuting(ActionExecutingContext context)
    {
        if (appSettingsKey.Value.EnableLogRequest)
        {
            var model = context.ActionArguments;
            var serialString = JsonConvert.SerializeObject(model);
            var url = context.HttpContext.Request.GetDisplayUrl();
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"{url} - Params: {serialString}");

            await logger.InfoAsync(stringBuilder.ToString());
        }

        base.OnActionExecuting(context);
    }
}