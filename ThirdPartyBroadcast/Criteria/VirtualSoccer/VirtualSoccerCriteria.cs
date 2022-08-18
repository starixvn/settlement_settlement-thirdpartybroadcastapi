using Fanex.Data;
using Fanex.Data.Repository;

namespace ThirdPartyBroadcast.Criteria.VirtualSoccer
{
    public class VirtualSoccerCriteria : CalculateCriteria
    {
        public int Fth { get; set; }

        public int Fta { get; set; }

        public string Status { get; set; }

        public string Action { get; set; }

        public override string GetSettingKey() => $"NAP_Admin_Settlement_VS_180_{DbServer}";

        public override bool IsValid() => MatchId > 0;
    }
}