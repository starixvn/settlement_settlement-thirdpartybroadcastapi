using MediatR;
using ThirdPartyBroadcast.Models;

namespace ThirdPartyBroadcast.Requests
{
    public class VirtualBetRadarRequest : IRequest<Result>
    {
        public int MatchId { get; set; }

        public string Action { get; set; }

        public int Hth { get; set; }

        public int Hta { get; set; }

        public int Fth { get; set; }

        public int Fta { get; set; }

        public string ResourceResult { get; set; }
    }
}