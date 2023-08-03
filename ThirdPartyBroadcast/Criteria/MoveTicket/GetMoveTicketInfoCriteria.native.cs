namespace ThirdPartyBroadcast.Criteria.MoveTicket;

public class GetMoveTicketInfoCriteria : CriteriaBase
{
    public int MatchId { get; set; }

    public override string GetSettingKey() => "NAP_Admin_Settlement_50_MoveTicketInfo";

    public override bool IsValid() => MatchId > 0;
}