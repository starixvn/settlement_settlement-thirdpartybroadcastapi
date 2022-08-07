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
    }
}
