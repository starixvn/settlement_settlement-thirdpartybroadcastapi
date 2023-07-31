namespace ThirdPartyBroadcast.Criteria.VirtualSoccer;

public class VRSoccerUpdateMatchCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    public string Command { get; set; }

    public override string GetSettingKey() => "NAP_UpdateMatch_Remark";

    public override bool IsValid() => MatchId > 0;
}