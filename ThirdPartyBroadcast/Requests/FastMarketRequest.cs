using MediatR;
using ThirdPartyBroadcast.Models;

namespace ThirdPartyBroadcast.Requests
{
    public class FastMarketRequest : IRequest<Result>
    {
        public int MatchId { get; set; }

        public string Action { get; set; }

        public bool IsReSettledTickets { get; set; }

        public string EventCounters { get; set; }
    }
}