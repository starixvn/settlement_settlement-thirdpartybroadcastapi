namespace ThirdPartyBroadcast.Criteria.MoveTicket;

public class MoveTicketCriteria : BaseCriteria
{
    public int MatchId { get; set; }

    public int AdminId { get; set; }

    public string Ip { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    public int ChildMatchId { get; set; }

    public string Resources { get; set; }

    public int TicketCount { get; set; }

    public override string GetSettingKey() => $"NAP_Admin_Settlement_50_MoveTicket_{DbServer}";

    public override bool IsValid() => MatchId > 0;
}