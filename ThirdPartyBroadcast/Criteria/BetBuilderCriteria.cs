using Fanex.Data;
using Fanex.Data.Repository;

namespace ThirdPartyBroadcast.Criteria
{
    public class BetBuilderCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public string Action { get; set; }

        public int AdminId { get; set; }

        public bool IsAuto { get; set; }

        public int VendorId { get; set; }

        public string Resources { get; set; }

        public string Ip { get; set; }

        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }

        [SpParam(Ignored = true)]
        public string DbServer { get; set; }

        public override string GetSettingKey() => $"NAP_Admin_Settlement_BetBuilder_Push_{DbServer}";

        public override bool IsValid() => MatchId > 0;
    }
}