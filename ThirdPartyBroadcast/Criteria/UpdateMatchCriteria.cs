namespace ThirdPartyBroadcast.Criteria;

public class UpdateMatchCriteria : INonQueryCommand
{
    public int MatchId { get; set; }

    public string Action { get; set; }

    public string Command { get; set; }

    public string SelectedResources { get; set; }

    public string GetSettingKey()
    {
        return "NAP_Admin_Match_Update";
    }

    public bool IsValid()
    {
        return MatchId > 0;
    }
}