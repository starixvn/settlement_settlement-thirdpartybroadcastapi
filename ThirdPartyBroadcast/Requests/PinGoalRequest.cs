using MediatR;
using ThirdPartyBroadcast.Models;

namespace ThirdPartyBroadcast.Requests
{
    public class PinGoalRequest : IRequest<Result>
    {
        public int MatchId { get; set; }

        public string Action { get; set; }

        public string BetStatus { get; set; }

        public bool IsReSettledTickets { get; set; }

        public string SelectedResources { get; set; }
    }
}