using Fanex.Data;
using Fanex.Data.Repository;

namespace ThirdPartyBroadcast.Criteria
{
    public class FastMarketCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public string Action { get; set; }

        public int AdminId { get; set; }

        public bool IsAuto { get; set; } = true;

        public string Ip { get; set; }

        public string Results { get; set; }

        public bool IsSettleDetermined { get; set; }

        public bool IsReSettledTickets { get; set; }

        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }

        [SpParam(Ignored = true)]
        public string DbServer { get; set; }

        public override string GetSettingKey() => $"NAP_Admin_Settlement_Fast_Market_Auto_1s_Push_{DbServer}";

        public override bool IsValid() => MatchId > 0;
    }
}