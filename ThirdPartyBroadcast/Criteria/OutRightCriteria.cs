using Fanex.Data;
using Fanex.Data.Repository;

namespace ThirdPartyBroadcast.Criteria
{
    public class OutRightCriteria : CalculateCriteria
    {
        public float DeadHeat { get; set; }

        public bool IsDeadHeat { get; set; }

        public override string GetSettingKey() => $"NAP_Admin_Settlement_Outright_{DbServer}";
    }
}