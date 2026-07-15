using Microsoft.AspNetCore.Mvc;

namespace iot_monitoring.Controllers
{
    public class StripeWebhookController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
