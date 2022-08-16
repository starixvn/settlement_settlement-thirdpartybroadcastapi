using Fanex.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyBroadcast.Criteria.VirtualBetRadar
{
    public class UpdateMatchBetRadarCriteria : NonQueryCommand
    {
        public int MatchId { get; set; }

        public string Action { get; set; }

        public int AdminId { get; set; }

        public bool IsAuto { get; set; }

        public int Hth { get; set; }

        public int Hta { get; set; }

        public int Fth { get; set; }

        public int Fta { get; set; }

        public string Resources { get; set; }

        public override string GetSettingKey()
        {
            return "NAP_Admin_Match_s193_Upd";
        }

        public override bool IsValid()
        {
            return MatchId > 0;
        }
    }
}