namespace ThirdPartyBroadcast.Requests;

public class VirtualKironRequest
{
    public int EventId { get; set; }

    public int AdminId { get; set; }

    public string Ip { get; set; } = string.Empty;

    public string Action { get; set; } = ActionName.Calculate;
}