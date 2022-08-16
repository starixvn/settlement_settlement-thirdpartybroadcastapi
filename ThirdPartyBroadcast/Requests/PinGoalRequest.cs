using MediatR;
using ThirdPartyBroadcast.Models;

namespace ThirdPartyBroadcast.Requests
{
    public class PinGoalRequest : IRequest<Result>
    {
        public int MatchId { get; set; }

        public int AdminId { get; set; }

        public string Action { get; set; }

        public string Ip { get; set; }

        public bool IsAuto { get; set; } = true;

        public string BetStatus { get; set; }

        public bool IsReSettledTickets { get; set; }

        public string SelectedResources { get; set; }
    }
}