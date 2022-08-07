using Fanex.Data;
using Fanex.Data.Repository;

namespace ThirdPartyBroadcast.Criteria
{
    public class NumberGameCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public  int DrawSeq { get; set; }

        public string Action { get; set; }

        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }

        [SpParam(Ignored = true)]
        public string DbServer { get; set; }

        public override string GetSettingKey() => $"NAP_Admin_Settlement_s161_{DbServer}";

        public override bool IsValid() => MatchId > 0;
    }
}
