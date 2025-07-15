using Microsoft.Extensions.Options;
using ONEbook.UM.Services;

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
        var isDeployed = await flagService.GetStatusOnCache(CacheKeys.BroadcastFlagCacheKey);
        var isFlagUnderMaintenance = await flagService.GetStatusOnCache(CacheKeys.BroadcastSettleUnderMaintenanceCacheKey);
        var productTypeOnCache = await flagService.GetProductTypeOnCache(CacheKeys.BroadcastProductCacheKey);
        var productTypeList = productTypeOnCache?.Split(",");
        var isMaintenance = await maintenanceService.IsUnderMaintenanceAsync();

        if ((isMaintenance && !isFlagUnderMaintenance) || (isDeployed && productTypeList.Contains(productType)))
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