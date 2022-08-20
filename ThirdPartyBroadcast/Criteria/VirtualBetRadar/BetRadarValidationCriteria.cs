using Fanex.Settlement.ScaleOut.Criteria;

namespace ThirdPartyBroadcast.Criteria.VirtualBetRadar
{
    public class BetRadarValidationCriteria : BaseCriteria
    {
        public int MatchId { get; set; }

        public string SelectedResources { get; set; }

        public override string GetSettingKey()
        {
            return $"NAP_Admin_Settlement_s193_Validation_{DbServer}";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}