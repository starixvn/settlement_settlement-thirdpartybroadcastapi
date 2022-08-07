using MediatR;
using ThirdPartyBroadcast.Models;

namespace ThirdPartyBroadcast.Requests
{
    public class BetBuilderRequest : IRequest<Result>
    {
        public int MatchId { get; set; }

        public string Resources { get; set; }

        public string Action { get; set; }

        public bool IsAuto { get; set; }
    }
}
