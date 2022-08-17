using Fanex.Data;
using Fanex.Data.Repository;

namespace ThirdPartyBroadcast.Criteria
{
    public class NumberGameCriteria : CalculateCriteria
    {
        public int DrawSeq { get; set; }

        public bool IsLive { get; set; }

        public override string GetSettingKey() => $"NAP_Admin_Settlement_s161_{DbServer}";
    }
}