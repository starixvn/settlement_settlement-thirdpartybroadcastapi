using MediatR;
using ThirdPartyBroadcast.Models;

namespace ThirdPartyBroadcast.Requests
{
    public class AutoVirtualBetRadarRequest : IRequest<Result>
    {
        public int MatchId { get; set; }

        public int AdminId { get; set; }

        public string Action { get; set; } = "CAL";

        public string Ip { get; set; } = string.Empty;

        public bool IsAuto { get; set; } = true;

        public int? Hth { get; set; }

        public int? Hta { get; set; }

        public int? Fth { get; set; }

        public int? Fta { get; set; }

        public string ResourceResult { get; set; } = string.Empty;
    }
}