using MediatR;
using ThirdPartyBroadcast.Models;

namespace ThirdPartyBroadcast.Requests
{
    public class AutoOutrightRequest : IRequest<Result>
    {
        public int MatchId { get; set; }

        public int AdminId { get; set; } = 5838421;

        public string Action { get; set; }

        public string Status { get; set; }

        public float DeadHeat { get; set; } = 1;

        public string EventDate { get; set; }
    }
}