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

        public override async void OnActionExecuted(ActionExecutedContext actionExecutedContext)
        {
            if (!appSettingsKey.LogRequest)
            {
                return;
            }
            var context = actionExecutedContext.HttpContext;
            var request = context.Request;
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Url: {request.GetDisplayUrl()}");
            stringBuilder.AppendLine($"UserAgent: {request.Headers["User-Agent"]}");
            stringBuilder.AppendLine($"Username: {context.User.Identity.Name}");
            if (actionExecutedContext.Result != null)
            {
                var objectResult = actionExecutedContext.Result as OkObjectResult;
                var result = JsonConvert.SerializeObject(objectResult.Value);
                stringBuilder.AppendLine(result);
            }
            await logger.InfoAsync(stringBuilder.ToString());

            base.OnActionExecuted(actionExecutedContext);
        }

        public override async void OnActionExecuting(ActionExecutingContext context)
        {
            if (!appSettingsKey.LogRequest)
            {
                return;
            }
            var model = context.ActionArguments;
            var serialString = JsonConvert.SerializeObject(model);
            await logger.InfoAsync(serialString);

            base.OnActionExecuting(context);
        }
    }
}