namespace ThirdPartyBroadcast.Requests;

public class MoveTicketRequest : IRequest<Result>
{
    public int MatchId { get; set; }

    public int AdminId { get; set; }

    public string Ip { get; set; } = string.Empty;
}