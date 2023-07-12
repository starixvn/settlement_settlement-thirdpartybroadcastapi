namespace ThirdPartyBroadcast.Requests;

public class LeapGamingRequest : IRequest<Result>
{
    public int MatchId { get; set; }

    public int AdminId { get; set; }

    public string SelectedResources { get; set; }

    public string Ip { get; set; } = string.Empty;

    public string BetStatus { get; set; }

    public string Action { get; set; } = "CAL";

    public int SportType { get; set; } = 178;

    public bool IsReSettledTickets { get; set; }
}