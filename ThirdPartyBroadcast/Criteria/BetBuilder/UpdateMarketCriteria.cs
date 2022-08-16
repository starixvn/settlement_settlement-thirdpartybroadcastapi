using Fanex.Data.Repository;
using System;

namespace ThirdPartyBroadcast.Criteria.BetBuilder
{
    public class UpdateMarketCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public int VendorId { get; set; }

        public string Action { get; set; }

        public string Resources { get; set; }

        public string ResultStatusId { get; set; }

        public override string GetSettingKey()
        {
            return "NAP_Admin_ChangedMarket_Sel";
        }

        public override bool IsValid()
        {
            throw new NotImplementedException();
        }
    }
}