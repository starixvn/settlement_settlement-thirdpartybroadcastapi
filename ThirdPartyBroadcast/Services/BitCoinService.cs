namespace ThirdPartyBroadcast.Services;

using Newtonsoft.Json;

public interface BitCoinService
{
    Task<Result> Settle(AutoBitCoinRequest request);
}

public class BitCoinServiceImp : BitCoinService
{
    private readonly PublishMessageService publishMessageService;

    public BitCoinServiceImp(PublishMessageService publishMessageService)
    {
        this.publishMessageService = publishMessageService;
    }

    public async Task<Result> Settle(AutoBitCoinRequest request)
    {
        var data = JsonConvert.SerializeObject(request);

        return await publishMessageService.Publisher(ProductType.BitCoin, data);
    }
}