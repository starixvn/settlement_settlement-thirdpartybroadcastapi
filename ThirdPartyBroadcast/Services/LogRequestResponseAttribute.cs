using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using Fanex.Logging;
using Microsoft.AspNetCore.Http.Extensions;
using ILogger = Fanex.Logging.ILogger;
using ThirdPartyBroadcast.Models;
using Microsoft.Extensions.Options;

namespace ThirdPartyBroadcast.Services
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
            if (!appSettingsKey.LogRequest)
            {
                return;
            }
            var model = context.ActionArguments;
            var serialString = JsonConvert.SerializeObject(model);
            var Url = context.HttpContext.Request.GetDisplayUrl();
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Url: {Url}");
            stringBuilder.AppendLine($"Params: {serialString}");

            await logger.InfoAsync(stringBuilder.ToString());

            base.OnActionExecuting(context);
        }
    }
}