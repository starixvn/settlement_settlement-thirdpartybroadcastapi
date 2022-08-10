using MediatR;
using ThirdPartyBroadcast.Models;

namespace ThirdPartyBroadcast.Requests
{
    public class OutrightRequest : IRequest<Result>
    {
        public int MatchId { get; set; }

        public int AdminId { get; set; }

        public string Action { get; set; }

        public string Status { get; set; }

        public float DeadHeat { get; set; }

        public bool IsDeadHeat { get; set; }
    }
}