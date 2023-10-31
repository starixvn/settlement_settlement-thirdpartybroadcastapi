namespace ThirdPartyBroadcast.Handlers;

using Services;

public class GroupOneHandler :
        IRequestHandler<AutoHappyFiveRequest, Result>,
        IRequestHandler<AutoFastMarketRequest, Result>,
        IRequestHandler<AutoNumberGameRequest, Result>
{
    private readonly HappyFiveService happyFiveService;

    private readonly FastMarketService fastMarketService;

    private readonly NumberGameService numberGameService;

    public GroupOneHandler(
        HappyFiveService happyFiveService,
        FastMarketService fastMarketService,
        NumberGameService numberGameService)
    {
        this.happyFiveService = happyFiveService;
        this.fastMarketService = fastMarketService;
        this.numberGameService = numberGameService;
    }

    public async Task<Result> Handle(AutoHappyFiveRequest request, CancellationToken cancellationToken) => await happyFiveService.Settle(request);

    public async Task<Result> Handle(AutoFastMarketRequest request, CancellationToken cancellationToken) => await fastMarketService.Settle(request);

    public async Task<Result> Handle(AutoNumberGameRequest request, CancellationToken cancellationToken) => await numberGameService.Settle(request);
}