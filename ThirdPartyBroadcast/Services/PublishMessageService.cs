using MassTransit;

namespace ThirdPartyBroadcast.Services;

public interface PublishMessageService
{
    Task Publisher(string productType, string data);
}

public class PublishMessageServiceImp : PublishMessageService
{
    private const int DelayTimeStartBus = 4000;

    private readonly IBusControl busControl;

    private readonly FlagService flagService;

    public PublishMessageServiceImp(
        FlagService flagService,
        IBusControl busControl)
    {
        this.flagService = flagService;
        this.busControl = busControl;
    }

    public async Task Publisher(string productType, string data)
    {
        var isDeployed = await flagService.IsDeployedFromCache(CacheKeys.BroadcastFlagCacheKey);
        var productTypeOnCache = await flagService.GetProductTypeFromCache(CacheKeys.BroadcastProductCacheKey);
        var productTypeList = productTypeOnCache.Split(",");

        if (isDeployed && productTypeList.Contains(productType))
        {
            await PublishToWaiting(productType, data);
        }
        else
        {
            await PublishToProcessing(productType, data);
        }
    }

    private async Task PublishToWaiting(string productType, string data)
    {
        try
        {
            var source = new CancellationTokenSource(DelayTimeStartBus);
            await busControl.StartAsync(source.Token);
            await busControl.Publish<IBroadcastApiWaiting>(new
            {
                productType,
                data
            }, source.Token);
        }
        finally
        {
            await busControl.StopAsync();
        }
    }

    private async Task PublishToProcessing(string productType, string data)
    {
        try
        {
            var source = new CancellationTokenSource(DelayTimeStartBus);
            await busControl.StartAsync(source.Token);
            await busControl.Publish<IBroadcastApiProcessing>(new
            {
                productType,
                data
            }, source.Token);
        }
        finally
        {
            await busControl.StopAsync();
        }
    }
}