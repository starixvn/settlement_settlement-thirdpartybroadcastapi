using Fanex.Data;
using Fanex.Data.Repository;

namespace ThirdPartyBroadcast.Criteria
{
    public class PinGoalCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public string Action { get; set; }

        public int AdminId { get; set; }

        public bool IsAuto { get; set; }

        public string Ip { get; set; }

        public string BetStatus { get; set; }

        public bool IsReSettledTickets { get; set; }

        public string SelectedResources { get; set; }

        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }

        [SpParam(Ignored = true)]
        public string DbServer { get; set; }

        public override string GetSettingKey() => $"_{DbServer}";

        public override bool IsValid() => MatchId > 0;
    }
}