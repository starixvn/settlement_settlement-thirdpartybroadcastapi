using MediatR;
using ThirdPartyBroadcast.Models;

namespace ThirdPartyBroadcast.Requests
{
    public class BitCoinRequest : IRequest<Result>
    {
        public int MatchId { get; set; }

        public string Action { get; set; }

        public int ResultId { get; set; }

        public string GameResult { get; set; }
    }
}