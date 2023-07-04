namespace ThirdPartyBroadcast.Criteria;

public class WriteLogCriteria : NonQueryCommand
{
    public WriteLogCriteria(int matchId, string errorMessage, int actionId, int userId, string ip)
    {
        MatchId = matchId;
        Remark = errorMessage;
        ActionId = actionId;
        UserId = userId;
        IP = ip;
    }

    public int UserId { get; }

    public int MatchId { get; set; }

    public string Remark { get; set; }

    public string IP { get; set; }

    public int ActionId { get; }

    public int? FinalHomeScore { get; set; }

    public int? FinalAwayScore { get; set; }

    public int? HtHomeScore { get; set; }

    public int? HtAwayScore { get; set; }

    public override bool IsValid() => MatchId > 0;

    public override string GetSettingKey()
    {
        return "WriteLog_UserLog_Settle";
    }
}