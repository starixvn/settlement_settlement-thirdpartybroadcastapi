namespace ThirdPartyBroadcast.Requests;

public class AutoHappyFiveRequest
{
    public int MatchId { get; set; }

    public int AdminId { get; set; }

    public string Action { get; set; } = "CAL";

    public string Ip { get; set; } = string.Empty;

    public bool IsAuto { get; set; } = true;
}