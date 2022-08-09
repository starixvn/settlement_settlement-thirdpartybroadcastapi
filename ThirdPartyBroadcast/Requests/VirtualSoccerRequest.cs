using MediatR;
using ThirdPartyBroadcast.Models;

namespace ThirdPartyBroadcast.Requests
{
    public class VirtualSoccerRequest : IRequest<Result>
    {
        public int MatchId { get; set; }

        public string Action { get; set; }

        public int Fth { get; set; }

        public int Fta { get; set; }
    }
}