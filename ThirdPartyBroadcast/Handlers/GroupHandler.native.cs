namespace ThirdPartyBroadcast.Handlers;

using Services;

public class GroupHandler : IRequestHandler<MoveTicketRequest, Result>
{
    private readonly MoveTicketService moveTicketService;

    public GroupHandler(MoveTicketService moveTicketService)
    {
        this.moveTicketService = moveTicketService;
    }

    public async Task<Result> Handle(MoveTicketRequest request, CancellationToken cancellationToken) => await moveTicketService.MoveTicket(request);
}