using Fanex.Data.Repository;

namespace ThirdPartyBroadcast.Criteria.BitCoin
{
    internal class SendMailCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public int NotifyAction { get; set; }

        public override string GetSettingKey()
        {
            return "NAP_Admin_SendNotify_Bitcoin";
        }

        public override bool IsValid()
        {
            return MatchId > 0;
        }
    }
}