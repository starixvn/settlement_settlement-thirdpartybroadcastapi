namespace ThirdPartyBroadcast.Criteria.LeapGaming;

public class LeapGamingSendMailCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    public string CreditRecipients { get; set; }

    public string DepositRecipients { get; set; }

    public string CopyRecipients { get; set; }

    public override string GetSettingKey()
    {
        return "NAP_Admin_SendNotify_s178";
    }

    public override bool IsValid()
    {
        return MatchId > 0;
    }
}