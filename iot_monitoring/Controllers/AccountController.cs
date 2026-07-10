using iot_monitoring.Data;
using iot_monitoring.Services;
using iot_monitoring.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;

namespace iot_monitoring.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;

        public AccountController(AppDbContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == model.Username && u.IsActive);
            if (user == null)
            {
                ViewBag.ErrorMessage = "Invalid username or password.";
                return View(model);
            }
            bool isPasswordValid = _passwordService.VerifyPassword(model.Password, user.Password);
            if (!isPasswordValid)
            {
                ViewBag.ErrorMessage = "Invalid username or password.";
                return View(model);
            }
            return RedirectToAction("Index", "Devices");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Account");
        }
    }
}
