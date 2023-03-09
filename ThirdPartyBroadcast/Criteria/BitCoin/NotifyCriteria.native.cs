namespace ThirdPartyBroadcast.Criteria.BitCoin;

public class NotifyCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    public int NotifyAction { get; set; }

    public override string GetSettingKey()
    {
        return "NAP_Admin_SendNotify_Bitcoin";
    }

    public override bool IsValid()
    {
        return MatchId > 0;
    }
}