using Microsoft.Extensions.Options;

namespace ThirdPartyBroadcast.Services;

public interface PublishMessageService
{
    Task<Result> Publisher(string productType, string data);
}

public class PublishMessageServiceImp(
    FlagService flagService,
    ManageQueueService manageQueueService,
    IOptions<AppSettingsKey> appSettingsKey,
    IMaintenanceService maintenanceService)
    : PublishMessageService
{
    private const int DelayTime = 2000;

    public async Task<Result> Publisher(string productType, string data)
    {
        var isDeployed = await flagService.IsDeployedFromCache(CacheKeys.BroadcastFlagCacheKey);
        var productTypeOnCache = await flagService.GetProductTypeFromCache(CacheKeys.BroadcastProductCacheKey);
        var productTypeList = productTypeOnCache?.Split(",");

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

    private async Task ProcessWaitingQueue()
    {
        var productWaitingOnService = await manageQueueService.IsExistedItemInWaitingQueue(CacheKeys.BroadcastServiceProductWaitingCacheKey);
        if (productWaitingOnService)
        {
            await manageQueueService.ProcessWaitingQueue(CacheKeys.BroadcastServiceProductWaitingCacheKey);
        }

        var productWaitingOnApi = await manageQueueService.IsExistedItemInWaitingQueue(CacheKeys.BroadcastApiProductWaitingCacheKey);
        if (productWaitingOnApi)
        {
            await manageQueueService.ProcessWaitingQueue(CacheKeys.BroadcastApiProductWaitingCacheKey);
        }
    }

    private async Task<Result> RetryAndPublishToWaiting(string productType, string data)
    {
        var result = new Result();
        for (var i = 1; i <= appSettingsKey.Value.RetryCount; i++)
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

        for (var i = 1; i <= appSettingsKey.Value.RetryCount; i++)
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