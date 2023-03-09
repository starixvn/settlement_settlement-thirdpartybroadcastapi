namespace ThirdPartyBroadcast.Requests;

public class AutoVirtualSoccerRequest : IRequest<Result>
{
    public int MatchId { get; set; }

    public int AdminId { get; set; }

    public string Action { get; set; } = "CAL";

    public string Ip { get; set; } = string.Empty;

    public bool IsAuto { get; set; } = true;

    public string BetStatus { get; set; }

    public int? Fth { get; set; }

    public int? Fta { get; set; }
}