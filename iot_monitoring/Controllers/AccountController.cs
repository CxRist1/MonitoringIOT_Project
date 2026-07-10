using iot_monitoring.Data;
using iot_monitoring.Services;
using iot_monitoring.ViewModel;
using iot_monitoring.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using iot_monitoring.ViewModels;

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
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usernameExists = await _context.Users.AnyAsync(u => u.Username == model.Username);

            if (usernameExists)
            {
                ModelState.AddModelError("Username", "Username already exists.");
                return View(model);
            }

            var emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(model);
            }
            var user = new User
            {
                Username = model.Username,
                FullName = model.FullName,
                Email = model.Email,
                Password = _passwordService.HashPassword(model.Password),
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login", "Account");

        }

    }
}
