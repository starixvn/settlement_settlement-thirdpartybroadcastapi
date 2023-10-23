using Microsoft.Extensions.Options;

namespace ThirdPartyBroadcast.Services;

public interface PublishMessageService
{
    Task<Result> Publisher(string productType, string data);

    Task ProcessWaitingQueue();
}

public class PublishMessageServiceImp : PublishMessageService
{
    private readonly AppSettingsKey appSettingsKey;

    private readonly FlagService flagService;

    private readonly ManageQueueService manageQueueService;

    private readonly IMaintenanceService maintenanceService;

    private const int DelayTime = 2000;

    public PublishMessageServiceImp(
        FlagService flagService,
        ManageQueueService manageQueueService,
        IOptions<AppSettingsKey> appSettingsKey,
        IMaintenanceService maintenanceService)
    {
        this.flagService = flagService;
        this.manageQueueService = manageQueueService;
        this.appSettingsKey = appSettingsKey.Value;
        this.maintenanceService = maintenanceService;
    }

    public async Task<Result> Publisher(string productType, string data)
    {
        var isDeployed = await flagService.IsDeployedFromCache(CacheKeys.BroadcastFlagCacheKey);
        var productTypeOnCache = await flagService.GetProductTypeFromCache(CacheKeys.BroadcastProductCacheKey);
        var productTypeList = productTypeOnCache.Split(",");

        if (maintenanceService.IsMaintenance || (isDeployed && productTypeList.Contains(productType)))
        {
            return await RetryAndPublishToWaiting(productType, data);
        }

        if (!isDeployed)
        {
            await ProcessWaitingQueue();
        }

        return await RetryAndPublishToProcessing(productType, data);
    }

    public async Task ProcessWaitingQueue()
    {
        if (manageQueueService.IsExistedItemInWaitingQueue(CacheKeys.BroadcastServiceProductWaitingCacheKey))
        {
            await manageQueueService.ProcessWaitingQueue(CacheKeys.BroadcastServiceProductWaitingCacheKey);
        }

        if (manageQueueService.IsExistedItemInWaitingQueue(CacheKeys.BroadcastApiProductWaitingCacheKey))
        {
            await manageQueueService.ProcessWaitingQueue(CacheKeys.BroadcastApiProductWaitingCacheKey);
        }
    }

    private async Task<Result> RetryAndPublishToWaiting(string productType, string data)
    {
        var result = new Result();
        for (var i = 1; i <= appSettingsKey.RetryCount; i++)
        {
            result = await manageQueueService.PublishToWaiting(productType, data, i);
            if (result.IsSuccessful)
            {
                return result;
            }

            await Task.Delay(DelayTime);
        }

        return result;
    }

    private async Task<Result> RetryAndPublishToProcessing(string productType, string data)
    {
        var result = new Result();

        for (var i = 1; i <= appSettingsKey.RetryCount; i++)
        {
            result = await manageQueueService.PublishToProcessing(productType, data, i);
            if (result.IsSuccessful)
            {
                return result;
            }

            await Task.Delay(DelayTime);
        }

        return result;
    }
}