using Fanex.Data;
using Fanex.Data.Repository;

namespace ThirdPartyBroadcast.Criteria
{
    public class HappyFiveCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public int AdminId { get; set; }

        public string Action { get; set; }

        public bool Ip { get; set; }

        public bool IsAuto { get; set; }

        public string Resources { get; set; }

        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }

        [SpParam(Ignored = true)]
        public string DbServer { get; set; }

        public override string GetSettingKey() => $"NAP_Admin_Settlement_s164_{DbServer}";

        public override bool IsValid() => MatchId > 0;
    }
}
