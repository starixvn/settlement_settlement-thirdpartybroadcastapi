namespace ThirdPartyBroadcast.Criteria.LeapGaming;

public class LeapGamingSendMailCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    public string Credit_Recipients { get; set; }

    public string Deposit_Recipients { get; set; }

    public string Copy_recipients { get; set; }

    public override string GetSettingKey()
    {
        return "NAP_Admin_SendNotify_s178";
    }

    public override bool IsValid()
    {
        return MatchId > 0;
    }
}