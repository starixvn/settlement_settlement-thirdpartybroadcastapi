namespace ThirdPartyBroadcast.Handlers;

using Services;

public class GroupHandler :
        IRequestHandler<AutoNumberGameRequest, Result>,
        IRequestHandler<MoveTicketRequest, Result>
{
    private readonly NumberGameService numberGameService;

    private readonly MoveTicketService moveTicketService;

    public GroupHandler(
        NumberGameService numberGameService,
        MoveTicketService moveTicketService)
    {
        this.numberGameService = numberGameService;
        this.moveTicketService = moveTicketService;
    }

    public async Task<Result> Handle(AutoNumberGameRequest request, CancellationToken cancellationToken) => await numberGameService.Settle(request);

    public async Task<Result> Handle(MoveTicketRequest request, CancellationToken cancellationToken) => await moveTicketService.MoveTicket(request);
}