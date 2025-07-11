using Microsoft.AspNetCore.Mvc;
using ThirdPartyBroadcast.ActionFilters;
using ThirdPartyBroadcast.Services;

namespace ThirdPartyBroadcast.Controllers;

[ApiController]
[Route("api/third-party-broadcast")]
public class ThirdPartyBroadcastController(PublishMessageService publishMessageService)
    : ControllerBase
{
    [Route("test"), HttpGet]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public Task<IActionResult> Test()
    {
        return Task.FromResult<IActionResult>(Ok("OK-Test"));
    }

    [Route("number-game"), HttpPost]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public async Task<IActionResult> NumberGameSettle(AutoNumberGameRequest request)
    {
        var data = JsonConvert.SerializeObject(request);
        var result = await publishMessageService.Publisher(ProductType.NumberGame, data);
        return Ok(result);
    }

    [Route("fast-market"), HttpPost]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public async Task<IActionResult> FastMarketSettle(AutoFastMarketRequest request)
    {
        var data = JsonConvert.SerializeObject(request);
        var result = await publishMessageService.Publisher(ProductType.FastMarket, data);
        return Ok(result);
    }

    [Route("pin-goal"), HttpPost]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public async Task<IActionResult> PinGoalSettle(AutoPinGoalRequest request)
    {
        var data = JsonConvert.SerializeObject(request);
        var result = await publishMessageService.Publisher(ProductType.PinGoal, data);
        return Ok(result);
    }

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

    [Route("cock-fighting"), HttpPost]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public async Task<IActionResult> CockFighting(CockFightingRequest request)
    {
        var data = JsonConvert.SerializeObject(request);
        var result = await publishMessageService.Publisher(ProductType.CockFighting, data);
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

    [Route("bet-builder"), HttpPost]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public async Task<IActionResult> BetBuilderSettle(AutoBetBuilderRequest request)
    {
        var data = JsonConvert.SerializeObject(request);
        var result = await publishMessageService.Publisher(ProductType.BetBuilder, data);
        return Ok(result);
    }

    [Route("happy-five"), HttpPost]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public async Task<IActionResult> HappyFiveSettle(AutoHappyFiveRequest request)
    {
        var data = JsonConvert.SerializeObject(request);
        var result = await publishMessageService.Publisher(ProductType.HappyFive, data);
        return Ok(result);
    }

    [Route("virtual-sport-five"), HttpPost]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public async Task<IActionResult> VirtualSportFiveSettle(VirtualSportFiveRequest request)
    {
        var data = JsonConvert.SerializeObject(request);
        var result = await publishMessageService.Publisher(ProductType.VirtualSports5, data);
        return Ok(result);
    }

    [Route("virtual-racing"), HttpPost]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public async Task<IActionResult> VirtualRacing(VirtualRacingRequest request)
    {
        var data = JsonConvert.SerializeObject(request);
        var result = await publishMessageService.Publisher(ProductType.VirtualRacing, data);
        return Ok(result);
    }

    [Route("virtual-betby"), HttpPost]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public async Task<IActionResult> VirtualBetby(VirtualBetbyRequest request)
    {
        var data = JsonConvert.SerializeObject(request);
        var result = await publishMessageService.Publisher(ProductType.VirtualSports6, data);
        return Ok(result);
    }

    [Route("virtual-kiron"), HttpPost]
    [ServiceFilter(typeof(LogRequestAttribute))]
    public async Task<IActionResult> VirtualKiron(VirtualKironRequest request)
    {
        var data = JsonConvert.SerializeObject(request);
        var result = await publishMessageService.Publisher(ProductType.VirtualKiron, data);
        return Ok(result);
    }
}