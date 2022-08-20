using Fanex.Settlement.ScaleOut.Criteria;

namespace ThirdPartyBroadcast.Criteria.BitCoin
{
    public class BitCoinValidationCriteria : BaseCriteria
    {
        public override string GetSettingKey()
        {
            return $"NAP_Admin_Settlement_s175_Validation_{DbServer}";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}