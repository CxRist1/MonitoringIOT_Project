using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace iot_monitoring.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
