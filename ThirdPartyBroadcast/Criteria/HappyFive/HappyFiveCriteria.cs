using Fanex.Data;
using Fanex.Data.Repository;

namespace ThirdPartyBroadcast.Criteria.HappyFive
{
    public class HappyFiveCriteria : CalculateCriteria
    {
        public string Resources { get; set; }

        public override string GetSettingKey() => $"NAP_Admin_Settlement_s164_{DbServer}";
    }
}