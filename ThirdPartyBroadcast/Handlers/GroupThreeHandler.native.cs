using ThirdPartyBroadcast.Services;

namespace ThirdPartyBroadcast.Handlers;

public class GroupThreeHandler : IRequestHandler<MoveTicketRequest, Result>
{
    private readonly MoveTicketService moveTicketService;

    public GroupThreeHandler(MoveTicketService moveTicketService)
    {
        this.moveTicketService = moveTicketService;
    }

    public async Task<Result> Handle(MoveTicketRequest request, CancellationToken cancellationToken)
    {
        return await moveTicketService.MoveTicket(request);
    }
}