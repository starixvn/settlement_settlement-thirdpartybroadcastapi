using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
using MassTransit;
using Microsoft.Extensions.Options;

namespace ThirdPartyBroadcast.Services;

public interface ManageQueueService
{
    Task<Result> PublishToWaiting(string productType, string data, int index);

    Task<Result> PublishToProcessing(string productType, string data, int index);
}

public class ManageQueueServiceImp : ManageQueueService
{
    private readonly AppSettingsKey appSettingsKey;

    private readonly IPublishEndpoint publishEndpoint;

    public ManageQueueServiceImp(
        IOptions<AppSettingsKey> appSettingsKey,
        IPublishEndpoint publishEndpoint)
    {
        this.appSettingsKey = appSettingsKey.Value;
        this.publishEndpoint = publishEndpoint;
    }

    public async Task<Result> PublishToWaiting(string productType, string data, int index)
    {
        var result = new Result();

        try
        {
            await publishEndpoint.Publish<IBroadcastApiWaiting>(new
            {
                productType,
                data
            });
        }
        catch (Exception exception)
        {
            result.ErrorCode = 1000;
            result.ErrorMessage = $"Error from broad cast api: Match info {data}\n {exception.Message}";

            await WriteLog(exception, result.ErrorMessage, index);

            return result;
        }

        return result;
    }

    public async Task<Result> PublishToProcessing(string productType, string data, int index)
    {
        var result = new Result();

        try
        {
            await publishEndpoint.Publish<IBroadcastApiProcessing>(new
            {
                productType,
                data
            });
        }
        catch (Exception exception)
        {
            result.ErrorCode = 1000;
            result.ErrorMessage = $"Error from broad cast api: Match info {data}\n {exception.Message}";
            await WriteLog(exception, result.ErrorMessage, index);

            return result;
        }

        return result;
    }

    private async Task WriteLog(Exception exception, string errorMessage, int index)
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