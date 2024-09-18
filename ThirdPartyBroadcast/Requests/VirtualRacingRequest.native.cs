namespace ThirdPartyBroadcast.Requests;

public class VirtualRacingRequest
{
    public int EventId { get; set; }

    public int AdminId { get; set; }

    public string Action { get; set; } = ActionName.Calculate;

    public string Ip { get; set; } = string.Empty;
}