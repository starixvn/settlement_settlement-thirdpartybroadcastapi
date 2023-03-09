namespace ThirdPartyBroadcast.Criteria.VirtualBetRadar;

public class GetMatchStatusCriteria : CriteriaBase
{
    public int MatchId { get; set; }

    public override string GetSettingKey()
    {
        return "NAP_Set_Match_s193_Sel";
    }

    public override bool IsValid()
    {
        return MatchId > 0;
    }
}