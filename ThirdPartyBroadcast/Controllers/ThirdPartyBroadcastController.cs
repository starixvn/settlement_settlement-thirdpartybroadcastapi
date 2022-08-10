using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MediatR;
using ThirdPartyBroadcast.Requests;

namespace ThirdPartyBroadcast.Controllers
{
    [ApiController]
    [Route("api/third-party-broadcast")]
    public class ThirdPartyBroadcastController : ControllerBase
    {
        private readonly IMediator mediator;

        public ThirdPartyBroadcastController(IMediator mediator)
        {
            this.mediator = mediator;
        }
        
        [Route("happy-five"), HttpPost]
        public async Task<IActionResult> HappyFiveSettle(HappyFiveRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

        [Route("number-game"), HttpPost]
        public async Task<IActionResult> NumberGameSettle(NumberGameRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

        [Route("bit-coin"), HttpPost]
        public async Task<IActionResult> BitCoinSettle(BitCoinRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

        [Route("pin-goal"), HttpPost]
        public async Task<IActionResult> PinGoalSettle(PinGoalRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

        [Route("virtual-bet-radar"), HttpPost]
        public async Task<IActionResult> VirtualBetRadarSettle(VirtualBetRadarRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

        [Route("virtual-soccer"), HttpPost]
        public async Task<IActionResult> VirtualSoccerSettle(VirtualSoccerRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

        [Route("fast-market"), HttpPost]
        public async Task<IActionResult> FastMarketSettle(FastMarketRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

        [Route("bet-builder"), HttpPost]
        public async Task<IActionResult> BetBuilderSettle(BetBuilderRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

        [Route("outright"), HttpPost]
        public async Task<IActionResult> OutrightSettle(OutrightRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));
    }
}
