namespace ThirdPartyBroadcast.Models;

public class AppSettingsKey
{
    public List<string> SiteDbServer { get; set; }

    public bool EnableLogRequest { get; set; }

    public int RetryCount { get; set; }
}