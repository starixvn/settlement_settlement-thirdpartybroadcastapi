using MediatR;
using ThirdPartyBroadcast.Models;

namespace ThirdPartyBroadcast.Requests
{
    public class BetBuilderRequest : IRequest<Result>
    {
        public int MatchId { get; set; }

        public int AdminId { get; set; }

        public string Action { get; set; }

        public bool Ip { get; set; }

        public bool IsAuto { get; set; }

        public int VendorId { get; set; }

        public string Resources { get; set; }

    }
}
