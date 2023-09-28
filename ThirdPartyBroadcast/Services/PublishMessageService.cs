using MassTransit;
using Microsoft.Extensions.Options;

namespace ThirdPartyBroadcast.Services;

public interface PublishMessageService
{
    Task Publisher(string productType, string data);
}

public class PublishMessageServiceImp : PublishMessageService
{
    private readonly AppSettingsKey appSettingsKey;

    private readonly FlagService flagService;

    public PublishMessageServiceImp(
        FlagService flagService,
        IOptions<AppSettingsKey> appSettingsKey)
    {
        this.flagService = flagService;
        this.appSettingsKey = appSettingsKey.Value;
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
        var rabbitMq = appSettingsKey.RabbitMq;
        var busControl = Bus.Factory.CreateUsingRabbitMq(configure =>
        {
            configure.Host(rabbitMq.Hosting, rabbitMq.VirtualHost, h =>
            {
                h.Username(rabbitMq.Username);
                h.Password(rabbitMq.Password);
            });
        });

        try
        {
            await busControl.StartAsync();
            await busControl.Publish<IBroadcastApiWaiting>(new
            {
                productType,
                data
            });
        }
        finally
        {
            await busControl.StopAsync();
        }
    }

    private async Task PublishToProcessing(string productType, string data)
    {
        var rabbitMq = appSettingsKey.RabbitMq;
        var busControl = Bus.Factory.CreateUsingRabbitMq(configure =>
        {
            configure.Host(rabbitMq.Hosting, rabbitMq.VirtualHost, h =>
            {
                h.Username(rabbitMq.Username);
                h.Password(rabbitMq.Password);
            });
        });

        try
        {
            await busControl.StartAsync();
            await busControl.Publish<IBroadcastApiProcessing>(new
            {
                productType,
                data
            });
        }
        finally
        {
            await busControl.StopAsync();
        }
    }
}