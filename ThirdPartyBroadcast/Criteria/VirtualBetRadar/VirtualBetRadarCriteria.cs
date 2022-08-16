using Fanex.Data;
using Fanex.Data.Repository;

namespace ThirdPartyBroadcast.Criteria.VirtualBetRadar
{
    public class VirtualBetRadarCriteria : CalculateCriteria
    {
        public int Hth { get; set; }

        public int Hta { get; set; }

        public int Fth { get; set; }

        public int Fta { get; set; }

        public string ResourceResult { get; set; }

        public override string GetSettingKey() => $"NAP_Admin_Settlement_VS_BetRadar_Push_{DbServer}";
    }
}