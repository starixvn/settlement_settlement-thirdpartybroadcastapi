namespace ThirdPartyBroadcast.Criteria.MoveTicket;

public class UpdateChildMatchRemarkCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    public string Command => "C";

    public bool IsMovedMatch => true;

    public override string GetSettingKey() => "NAP_UpdateMatch_Remark";

    public override bool IsValid() => MatchId > 0;
}