using MassTransit;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ThirdPartyBroadcast.Services;

public interface ManageQueueService
{
    Task<Result> PublishToWaiting(string productType, string data, int index);

    Task<Result> PublishToProcessing(string productType, string data, int index);

    Task ProcessWaitingQueue();

    bool ExistItemInWaitingQueue();
}

public class ManageQueueServiceImp : ManageQueueService
{
    private readonly AppSettingsKey appSettingsKey;

    private readonly IPublishEndpoint publishEndpoint;
    private readonly IDatabase redisDatabase;
    private readonly WriteLogService writeLogService;

    private const int ErrorCode = 1000;

    private const string ErrorMessage = "Error from broad cast api: Match info";

    private const string ErrorProcessWaitingQueue = "Failed: Process waiting queue";

    private const string SpecifiedWord = ":space:";

    private static readonly SemaphoreSlim specialRequestSemaphore = new(1, 1);

    public ManageQueueServiceImp(
        IOptions<AppSettingsKey> appSettingsKey,
        IPublishEndpoint publishEndpoint,
        IConnectionMultiplexer connectionMultiplexer,
        WriteLogService writeLogService)
    {
        this.appSettingsKey = appSettingsKey.Value;
        this.publishEndpoint = publishEndpoint;
        redisDatabase = connectionMultiplexer.GetDatabase();
        this.writeLogService = writeLogService;
    }

    public async Task<Result> PublishToWaiting(string productType, string data, int index)
    {
        var result = new Result();

        try
        {
            redisDatabase.ListRightPush(appSettingsKey.WaitingRequestList, $"{productType}{SpecifiedWord}{data}");
        }
        catch (Exception exception)
        {
            result.ErrorCode = ErrorCode;
            result.ErrorMessage = $"{ErrorMessage} {data}\n {exception.Message}";

            writeLogService.WriteToFile(data);
            await writeLogService.WriteLog(exception, result.ErrorMessage, index, false);

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
            result.ErrorCode = ErrorCode;
            result.ErrorMessage = $"{ErrorMessage} {data}\n {exception.Message}";
            await writeLogService.WriteLog(exception, result.ErrorMessage, index, false);

            return result;
        }

        return result;
    }

    public async Task ProcessWaitingQueue()
    {
        await specialRequestSemaphore.WaitAsync();

        try
        {
            while (redisDatabase.ListLength(appSettingsKey.WaitingRequestList) > 0)
            {
                await PushWaitingItemBackToQueue();
            }
        }
        catch (Exception e)
        {
            await writeLogService.WriteLog(e, ErrorProcessWaitingQueue, 0, true);
        }

        specialRequestSemaphore.Release();
    }

    private async Task PushWaitingItemBackToQueue()
    {
        var item = redisDatabase.ListRange(appSettingsKey.WaitingRequestList, 0, 0)[0];
        var value = ((string)item)?.Split(SpecifiedWord);
        if (value is { Length: 2 })
        {
            await publishEndpoint.Publish<IBroadcastApiProcessing>(new
            {
                productype = value[0],
                data = value[1]
            });
        }

        redisDatabase.ListLeftPop(appSettingsKey.WaitingRequestList);
    }

    public bool ExistItemInWaitingQueue()
    {
        return redisDatabase.ListLength(appSettingsKey.WaitingRequestList) > 0;
    }
}