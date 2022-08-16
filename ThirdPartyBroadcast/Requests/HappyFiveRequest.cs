using MediatR;
using ThirdPartyBroadcast.Models;

namespace ThirdPartyBroadcast.Requests
{
    public class HappyFiveRequest : IRequest<Result>
    {
        public int MatchId { get; set; }

        public int AdminId { get; set; }

        public string Action { get; set; } = "CAL";

        public string Ip { get; set; } = "";

        public bool IsAuto { get; set; } = true;
    }
}