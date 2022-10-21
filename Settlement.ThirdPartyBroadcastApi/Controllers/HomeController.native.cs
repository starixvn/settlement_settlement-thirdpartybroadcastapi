using Microsoft.AspNetCore.Mvc;

namespace ThirdPartyBroadcastApi.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Content("OK");
        }
    }
}
