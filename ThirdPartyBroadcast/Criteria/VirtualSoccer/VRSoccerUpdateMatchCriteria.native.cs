namespace ThirdPartyBroadcast.Criteria.VirtualSoccer;

public class VrSoccerUpdateMatchCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    public string Command { get; set; }

    public override string GetSettingKey()
    {
        return "NAP_updateMatch_remark";
    }

    public override bool IsValid()
    {
        return MatchId > 0;
    }
}