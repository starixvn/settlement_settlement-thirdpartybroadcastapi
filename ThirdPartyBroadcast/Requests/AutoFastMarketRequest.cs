using MediatR;
using ThirdPartyBroadcast.Models;

namespace ThirdPartyBroadcast.Requests
{
    public class AutoFastMarketRequest : IRequest<Result>
    {
        public int MatchId { get; set; }

        public int AdminId { get; set; } = 0;

        public string Action { get; set; }

        public string Ip { get; set; } = "";

        public bool IsAuto { get; set; }

        public bool IsReSettledTickets { get; set; } = true;

        public bool IsSettleDetermined { get; set; }

        public string EventCounters { get; set; }

        public string Results { get; set; }
    }
}