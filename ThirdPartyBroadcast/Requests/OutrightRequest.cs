using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdPartyBroadcast.Requests
{
    internal class OutrightRequest
    {
        public int MatchId { get; set; }

        public string Action { get; set; }

        public string Status { get; set; }

        public float DeadHeat { get; set; }

        public bool IsDeadHeat { get; set; }
    }
}