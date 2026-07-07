using Microsoft.AspNetCore.Mvc;

namespace iot_monitoring.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
