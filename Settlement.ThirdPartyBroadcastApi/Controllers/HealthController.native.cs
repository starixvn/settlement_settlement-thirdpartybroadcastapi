using Microsoft.AspNetCore.Mvc;

namespace ThirdPartyBroadcastApi.Controllers
{
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Content("OK");
        }
    }
}