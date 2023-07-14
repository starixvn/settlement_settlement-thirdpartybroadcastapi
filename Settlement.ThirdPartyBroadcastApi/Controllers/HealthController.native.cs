using Microsoft.AspNetCore.Mvc;

namespace Settlement.ThirdPartyBroadcastApi.Controllers;

public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        return Content("Swarm");
    }
}