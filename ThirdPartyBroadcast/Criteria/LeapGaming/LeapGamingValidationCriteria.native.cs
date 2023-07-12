namespace ThirdPartyBroadcast.Criteria.LeapGaming;

public class LeapGamingValidationCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    public string Action { get; set; }

    public string SelectedResources { get; set; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    public override string GetSettingKey()
    {
        return "NAP_Admin_Settlement_s178_Validation";
    }

    public override bool IsValid()
    {
        return MatchId > 0;
    }
}