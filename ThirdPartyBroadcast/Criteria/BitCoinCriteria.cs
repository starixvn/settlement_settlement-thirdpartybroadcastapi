using Fanex.Data;
using Fanex.Data.Repository;

namespace ThirdPartyBroadcast.Criteria
{
    internal class BitCoinCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public string Action { get; set; }

        public int AdminId { get; set; }

        public bool IsAuto { get; set; }

        public string GameResult { get; set; }

        public int ResultId { get; set; }

        public string Ip { get; set; }

        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }

        [SpParam(Ignored = true)]
        public string DbServer { get; set; }

        public override string GetSettingKey() => $"NAP_Admin_Settlement_s175_{DbServer}";

        public override bool IsValid() => MatchId > 0;
    }
}