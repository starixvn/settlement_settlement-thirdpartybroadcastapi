using MediatR;
using ThirdPartyBroadcast.Models;

namespace ThirdPartyBroadcast.Requests
{
    public class BitCoinRequest : IRequest<Result>
    {
        public int MatchId { get; set; }

        public int AdminId { get; set; }

        public string Action { get; set; }

        public string Ip { get; set; }

        public bool IsAuto { get; set; }

        public int ResultId { get; set; }

        public string GameResult { get; set; }
    }
}