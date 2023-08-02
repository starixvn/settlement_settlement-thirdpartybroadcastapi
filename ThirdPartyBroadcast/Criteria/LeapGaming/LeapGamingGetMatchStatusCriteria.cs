namespace ThirdPartyBroadcast.Criteria.LeapGaming;

public class LeapGamingGetMatchStatusCriteria : CriteriaBase
{
    public int MatchId { get; set; }

    public override string GetSettingKey()
    {
        return "NAP_Set_Match_s178_Sel";
    }

    public override bool IsValid()
    {
        return MatchId > 0;
    }
}