using Microsoft.AspNetCore.Mvc;
using ThirdPartyBroadcast.ActionFilters;
using ThirdPartyBroadcast.Services;

namespace ThirdPartyBroadcast.Controllers;

[ApiController]
[Route("api/third-party-broadcast")]
public class ThirdPartyBroadcastController : ControllerBase
{
    private readonly IMediator mediator;

    private readonly PublishMessageService publishMessageService;

    public ThirdPartyBroadcastController(
        IMediator mediator,
        PublishMessageService publishMessageService)
    {
        this.mediator = mediator;
        this.publishMessageService = publishMessageService;
    }

    [Route("happy-five"), HttpPost]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public async Task<IActionResult> HappyFiveSettle(AutoHappyFiveRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

    [Route("number-game"), HttpPost]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public async Task<IActionResult> NumberGameSettle(AutoNumberGameRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

    [Route("pin-goal"), HttpPost]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public async Task<IActionResult> PinGoalSettle(AutoPinGoalRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

    [Route("fast-market"), HttpPost]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public async Task<IActionResult> FastMarketSettle(AutoFastMarketRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

    [Route("bet-builder"), HttpPost]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public async Task<IActionResult> BetBuilderSettle(AutoBetBuilderRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

    [Route("move-tickets"), HttpPost]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public async Task<IActionResult> MoveTickets(MoveTicketRequest request) => Ok(await mediator.Send(request).ConfigureAwait(false));

    [Route("virtual-bet-radar"), HttpPost]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public async Task<IActionResult> VirtualBetRadarSettle(VirtualBetRadarRequest request)
    {
        var data = JsonConvert.SerializeObject(request);
        var result = await publishMessageService.Publisher(ProductType.VirtualBetRadar, data);
        return Ok(result);
    }

    [Route("virtual-soccer"), HttpPost]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public async Task<IActionResult> VirtualSoccerSettle(AutoVirtualSoccerRequest request)
    {
        var data = JsonConvert.SerializeObject(request);
        var result = await publishMessageService.Publisher(ProductType.VirtualSports, data);
        return Ok(result);
    }

    [Route("outright"), HttpPost]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public async Task<IActionResult> OutrightSettle(OutrightRequest request)
    {
        var data = JsonConvert.SerializeObject(request);
        var result = await publishMessageService.Publisher(ProductType.Outright, data);
        return Ok(result);
    }

    [Route("leap-gaming"), HttpPost]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public async Task<IActionResult> LeapGaming(LeapGamingRequest request)
    {
        var data = JsonConvert.SerializeObject(request);
        var result = await publishMessageService.Publisher(ProductType.LeapGaming, data);
        return Ok(result);
    }

    [Route("bit-coin"), HttpPost]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public async Task<IActionResult> BitCoinSettle(BitcoinRequest request)
    {
        var data = JsonConvert.SerializeObject(request);
        var result = await publishMessageService.Publisher(ProductType.BitCoin, data);
        return Ok(result);
    }
}