using System.Collections.Generic;
using MediatR;
using ThirdPartyBroadcast.Models;

namespace ThirdPartyBroadcast.Requests
{
    public class HappyFiveRequest : IRequest<Result>
    {
        public int MatchId { get; set; }

        public List<BallModel> Balls { get; set; }

        public string Action { get; set; }

        public bool IsAuto { get; set; }
    }
}
