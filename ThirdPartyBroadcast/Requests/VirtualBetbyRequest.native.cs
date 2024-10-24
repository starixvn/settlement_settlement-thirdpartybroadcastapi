namespace ThirdPartyBroadcast.Requests;

public class VirtualBetbyRequest
{
    public int MatchId { get; set; }

    public int AdminId { get; set; }

    public string SelectedResources { get; set; }

    public string Ip { get; set; } = string.Empty;

    public string Action { get; set; } = ActionName.Calculate;

    public bool IsReSettledTickets { get; set; }
}