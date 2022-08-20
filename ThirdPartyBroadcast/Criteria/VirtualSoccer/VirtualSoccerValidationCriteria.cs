using Fanex.Settlement.ScaleOut.Criteria;

namespace ThirdPartyBroadcast.Criteria.VirtualSoccer
{
    public class VirtualSoccerValidationCriteria : BaseCriteria
    {
        public int MatchId { get; set; }

        public override string GetSettingKey()
        {
            return $"NAP_Admin_Settlement_VS1_Validation_{DbServer}";
        }

        public override bool IsValid()
        {
            return MatchId > 0;
        }
    }
}