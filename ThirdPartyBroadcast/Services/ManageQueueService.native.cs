using MassTransit;
using StackExchange.Redis;

namespace ThirdPartyBroadcast.Services;

public interface ManageQueueService
{
    Task<Result> PublishToWaiting(string productType, string data, int index);

    Task<Result> PublishToProcessing(string productType, string data, int index);

    Task ProcessWaitingQueue(string key);

    bool IsExistedItemInWaitingQueue(string key);
}

public class ManageQueueServiceImp : ManageQueueService
{
    private readonly IPublishEndpoint publishEndpoint;

    private readonly IConnectionMultiplexer connectionMultiplexer;

    private readonly WriteLogService writeLogService;

    private const int ErrorCode = 1000;

    private const string ErrorMessage = "Error from broad cast api: Match info";

    private const string SpecifiedWord = ":space:";

    private static readonly SemaphoreSlim SpecialRequestSemaphore = new(1, 1);

    public ManageQueueServiceImp(
        IPublishEndpoint publishEndpoint,
        IConnectionMultiplexer connectionMultiplexer,
        WriteLogService writeLogService)
    {
        this.publishEndpoint = publishEndpoint;
        this.connectionMultiplexer = connectionMultiplexer;
        this.writeLogService = writeLogService;
    }

    private IDatabase Database => connectionMultiplexer.GetDatabase();

    public async Task<Result> PublishToWaiting(string productType, string data, int index)
    {
        var result = new Result();

        try
        {
            await Database.ListRightPushAsync(CacheKeys.BroadcastApiProductWaitingCacheKey, $"{productType}{SpecifiedWord}{data}");
        }
        catch (Exception exception)
        {
            result.ErrorCode = ErrorCode;
            result.ErrorMessage = $"{ErrorMessage} {data}\n {exception.Message}";
            await writeLogService.WriteLog(exception, result.ErrorMessage, index);

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
            await writeLogService.WriteLog(exception, result.ErrorMessage, index);

            return result;
        }

        return result;
    }

    public async Task ProcessWaitingQueue(string key)
    {
        await SpecialRequestSemaphore.WaitAsync();

        while (IsExistedItemInWaitingQueue(key))
        {
            await PushWaitingItemBackToQueue(key);
        }

        SpecialRequestSemaphore.Release();
    }

    private async Task PushWaitingItemBackToQueue(string key)
    {
        var item = (await Database.ListRangeAsync(key))[0];
        var value = ((string)item)?.Split(SpecifiedWord);
        if (value is { Length: 2 })
        {
            var productType = value[0];
            var data = value[1];

            await publishEndpoint.Publish<IBroadcastApiProcessing>(new
            {
                productType,
                data
            });
        }

        await Database.ListLeftPopAsync(key);
    }

    public bool IsExistedItemInWaitingQueue(string key)
    {
        return Database.ListLengthAsync(key).Result > 0;
    }
}