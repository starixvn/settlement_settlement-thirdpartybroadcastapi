namespace ThirdPartyBroadcast.Models;

public class AppSettingsKey
{
    public List<string> SiteDbServer { get; set; }

    public bool EnableLogRequest { get; set; }

    public RabbitMq RabbitMq { get; set; }

    public string WaitingRequestList { get; set; }

    public int RetryCount { get; set; }
}