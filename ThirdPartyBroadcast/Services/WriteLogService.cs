using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
using Microsoft.Extensions.Options;

namespace ThirdPartyBroadcast.Services;

public interface WriteLogService
{
    Task WriteLog(Exception exception, string errorMessage, int index);
}

public class WriteLogServiceImp : WriteLogService
{
    private readonly AppSettingsKey appSettingsKey;

    public WriteLogServiceImp(IOptions<AppSettingsKey> options)
    {
        appSettingsKey = options.Value;
    }

    public async Task WriteLog(Exception exception, string errorMessage, int index)
    {
        if (index == appSettingsKey.RetryCount)
        {
            var logMessage = new LogMessage(errorMessage)
                .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(exception))
                .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(exception))
                .ToString();
            await LogService.ErrorAsync(logMessage);
        }
    }
}