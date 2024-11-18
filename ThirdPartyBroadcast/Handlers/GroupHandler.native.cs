namespace ThirdPartyBroadcast.Handlers;

using Services;

public class GroupHandler(MoveTicketService moveTicketService) : IRequestHandler<MoveTicketRequest, Result>
{
    public async Task<Result> Handle(MoveTicketRequest request, CancellationToken cancellationToken) => await moveTicketService.MoveTicket(request);
}