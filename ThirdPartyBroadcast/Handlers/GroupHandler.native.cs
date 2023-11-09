namespace ThirdPartyBroadcast.Handlers;

using Services;

public class GroupHandler :
        IRequestHandler<AutoFastMarketRequest, Result>,
        IRequestHandler<AutoNumberGameRequest, Result>,
        IRequestHandler<MoveTicketRequest, Result>
{
    private readonly FastMarketService fastMarketService;

    private readonly NumberGameService numberGameService;

    private readonly MoveTicketService moveTicketService;

    public GroupHandler(
        FastMarketService fastMarketService,
        NumberGameService numberGameService,
        MoveTicketService moveTicketService)
    {
        this.fastMarketService = fastMarketService;
        this.numberGameService = numberGameService;
        this.moveTicketService = moveTicketService;
    }

    public async Task<Result> Handle(AutoFastMarketRequest request, CancellationToken cancellationToken) => await fastMarketService.Settle(request);

    public async Task<Result> Handle(AutoNumberGameRequest request, CancellationToken cancellationToken) => await numberGameService.Settle(request);

    public async Task<Result> Handle(MoveTicketRequest request, CancellationToken cancellationToken) => await moveTicketService.MoveTicket(request);
}