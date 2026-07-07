using Microsoft.AspNetCore.Mvc;

namespace iot_monitoring.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
