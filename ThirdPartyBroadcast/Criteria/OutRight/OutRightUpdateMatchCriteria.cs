using Fanex.Data.Repository;
using System;

namespace ThirdPartyBroadcast.Criteria.OutRight
{
    public class OutRightUpdateMatchCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public string Status { get; set; }

        public string Action { get; set; }

        public override string GetSettingKey()
        {
            return "NAP_Admin_Outright_Update";
        }

        public override bool IsValid()
        {
            return MatchId > 0;
        }
    }
}