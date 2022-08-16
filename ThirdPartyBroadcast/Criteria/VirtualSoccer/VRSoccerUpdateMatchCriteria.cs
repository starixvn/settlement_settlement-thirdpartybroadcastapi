using Fanex.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyBroadcast.Criteria.VirtualSoccer
{
    public class VRSoccerUpdateMatchCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public string Action { get; set; }

        public int AdminId { get; set; }

        public int Fth { get; set; }

        public int Fta { get; set; }

        public override string GetSettingKey()
        {
            return "NAP_Admin_Match_VS1_Upd";
        }

        public override bool IsValid()
        {
            return MatchId > 0;
        }
    }
}