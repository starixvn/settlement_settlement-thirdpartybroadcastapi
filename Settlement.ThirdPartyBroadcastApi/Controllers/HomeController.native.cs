using Microsoft.AspNetCore.Mvc;

namespace Settlement.ThirdPartyBroadcastApi.Controllers;

public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        return Content("OK");
    }
}