using Fanex.Data;
using Fanex.Data.Repository;

namespace ThirdPartyBroadcast.Criteria.BetBuilder
{
    public class BetBuilderCriteria : CalculateCriteria
    {
        public int VendorId { get; set; }

        public string Resources { get; set; }

        public string SelectedMarket { get; set; }

        public override string GetSettingKey() => $"NAP_Admin_Settlement_BetBuilder_Push_{DbServer}";
    }
}