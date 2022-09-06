using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Text;
using Fanex.Logging;
using Microsoft.AspNetCore.Http.Extensions;
using ILogger = Fanex.Logging.ILogger;
using ThirdPartyBroadcast.Models;
using Microsoft.Extensions.Options;

namespace ThirdPartyBroadcast.ActionFilters
{
    public class LogRequestResponseAttribute : ActionFilterAttribute
    {
        private readonly ILogger logger;

        private readonly AppSettingsKey appSettingsKey;

        public LogRequestResponseAttribute(IOptions<AppSettingsKey> appSettingsKey)
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
}