using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MediatR;
using ThirdPartyBroadcast.Requests;
using ThirdPartyBroadcast.Services;

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
        [ServiceFilter(typeof(LogRequestResponseAttribute))]
        public async Task<IActionResult> HappyFiveSettle(AutoHappyFiveRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

        [Route("number-game"), HttpPost]
        [ServiceFilter(typeof(LogRequestResponseAttribute))]
        public async Task<IActionResult> NumberGameSettle(AutoNumberGameRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

        [Route("bit-coin"), HttpPost]
        [ServiceFilter(typeof(LogRequestResponseAttribute))]
        public async Task<IActionResult> BitCoinSettle(AutoBitCoinRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

        [Route("pin-goal"), HttpPost]
        [ServiceFilter(typeof(LogRequestResponseAttribute))]
        public async Task<IActionResult> PinGoalSettle(AutoPinGoalRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

        [Route("virtual-bet-radar"), HttpPost]
        [ServiceFilter(typeof(LogRequestResponseAttribute))]
        public async Task<IActionResult> VirtualBetRadarSettle(AutoVirtualBetRadarRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

        [Route("virtual-soccer"), HttpPost]
        [ServiceFilter(typeof(LogRequestResponseAttribute))]
        public async Task<IActionResult> VirtualSoccerSettle(AutoVirtualSoccerRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

        [Route("fast-market"), HttpPost]
        [ServiceFilter(typeof(LogRequestResponseAttribute))]
        public async Task<IActionResult> FastMarketSettle(AutoFastMarketRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

        [Route("bet-builder"), HttpPost]
        [ServiceFilter(typeof(LogRequestResponseAttribute))]
        public async Task<IActionResult> BetBuilderSettle(AutoBetBuilderRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

        [Route("outright"), HttpPost]
        [ServiceFilter(typeof(LogRequestResponseAttribute))]
        public async Task<IActionResult> OutrightSettle(AutoOutrightRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));
    }
}