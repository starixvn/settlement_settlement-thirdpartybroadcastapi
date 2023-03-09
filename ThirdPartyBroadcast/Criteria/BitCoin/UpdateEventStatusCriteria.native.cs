namespace ThirdPartyBroadcast.Criteria.BitCoin;

public class UpdateEventStatusCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    public string Action { get; set; }

    public override string GetSettingKey()
    {
        return "NAP_Admin_Match3rd_Update";
    }

    public override bool IsValid()
    {
        return MatchId > 0;
    }
}