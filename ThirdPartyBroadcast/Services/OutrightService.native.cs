namespace ThirdPartyBroadcast.Services;

using Newtonsoft.Json;

public interface OutrightService
{
    Task<Result> Settle(AutoOutrightRequest request);
}

public class OutrightServiceImp : OutrightService
{
    private readonly PublishMessageService publishMessageService;

    public OutrightServiceImp(PublishMessageService publishMessageService)
    {
        this.publishMessageService = publishMessageService;
    }

    public async Task<Result> Settle(AutoOutrightRequest request)
    {
        var data = JsonConvert.SerializeObject(request);

        return await publishMessageService.Publisher(ProductType.Outright, data);
    }
}