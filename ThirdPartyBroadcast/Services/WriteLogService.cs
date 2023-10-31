using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
using Microsoft.Extensions.Options;

namespace ThirdPartyBroadcast.Services;

public interface WriteLogService
{
    Task WriteLog(Exception exception, string errorMessage, int index, bool logDirectly);

    void WriteToFile(string data);
}

public class WriteLogServiceImp : WriteLogService
{
    private readonly AppSettingsKey appSettingsKey;

    private const string FileName = "App_Data/RequestPushRedisFailed.txt";

    public WriteLogServiceImp(IOptions<AppSettingsKey> options)
    {
        appSettingsKey = options.Value;
    }

    public async Task WriteLog(Exception exception, string errorMessage, int index, bool logDirectly)
    {
        if (logDirectly || index == appSettingsKey.RetryCount)
        {
            var logMessage = new LogMessage(errorMessage)
                .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(exception))
                .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(exception))
                .ToString();
            await LogService.ErrorAsync(logMessage);
        }
    }

    public void WriteToFile(string data)
    {
        var contentToAppend = $"Request: {data} - Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}";

        using var writer = new StreamWriter(FileName, true);
        writer.WriteLine(contentToAppend);
    }
}