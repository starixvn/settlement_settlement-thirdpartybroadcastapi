namespace ThirdPartyBroadcast.Requests;

public class AutoNumberGameRequest
{
    public int MatchId { get; set; }

    public int AdminId { get; set; } = 148;

    public string Action { get; set; } = ActionName.Calculate;

    public string Ip { get; set; }

    public bool IsAuto { get; set; } = true;

    public int DrawSeq { get; set; }

    public bool IsLive { get; set; }

    public bool IsLoopTempTable { get; set; }
}