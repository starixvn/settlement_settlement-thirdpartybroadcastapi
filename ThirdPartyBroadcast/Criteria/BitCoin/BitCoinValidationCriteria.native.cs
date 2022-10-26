using Fanex.Data.Repository;

namespace ThirdPartyBroadcast.Criteria.BitCoin
{
    public class BitCoinEventStatusCriteria : CriteriaBase
    {
        public int MatchId { get; set; }

        public override string GetSettingKey()
        {
            return "NAP_Set_Match3rdSel";
        }

        public override bool IsValid()
        {
            return MatchId > 0;
        }
    }
}