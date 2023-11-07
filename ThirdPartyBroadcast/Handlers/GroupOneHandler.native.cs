namespace ThirdPartyBroadcast.Handlers;

using Services;

public class GroupOneHandler :
        IRequestHandler<AutoFastMarketRequest, Result>,
        IRequestHandler<AutoNumberGameRequest, Result>
{
    private readonly FastMarketService fastMarketService;

    private readonly NumberGameService numberGameService;

    public GroupOneHandler(
        FastMarketService fastMarketService,
        NumberGameService numberGameService)
    {
        this.fastMarketService = fastMarketService;
        this.numberGameService = numberGameService;
    }

    public async Task<Result> Handle(AutoFastMarketRequest request, CancellationToken cancellationToken) => await fastMarketService.Settle(request);

    public async Task<Result> Handle(AutoNumberGameRequest request, CancellationToken cancellationToken) => await numberGameService.Settle(request);
}