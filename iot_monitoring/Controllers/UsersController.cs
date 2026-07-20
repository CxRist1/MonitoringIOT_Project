using iot_monitoring.Data;
using iot_monitoring.Models;
using iot_monitoring.Services;
using iot_monitoring.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iot_monitoring.Controllers

{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;
        public UsersController(
            AppDbContext context,
            PasswordService passwordService )
        {
            _context = context;
            _passwordService = passwordService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? role)
        {
            var qurey = _context.Users
                .AsNoTracking()
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(role))
            {
                qurey = qurey.Where(u => u.Role == role);
            }

            var users = await qurey
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.SelectedRole = role;

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["UserError"] =
                    "Please complete the user information correctly";
                TempData["OpenCreateUserModel"] = true;
                return RedirectToAction(nameof(Index));
            }
            var normalizedUsername = model.Username.Trim();
            var normakizedEmail = model.Email.Trim();

            var usernameExists = await _context.Users
                .AnyAsync(u => u.Username == normalizedUsername);

            if (usernameExists)
            {
                TempData["UserError"] =
                    "The username is already in use";

                TempData["OpenCreateUserModal"] = true;
                return RedirectToAction(nameof(Index));
            }

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == normakizedEmail);

            if (emailExists)
            {
                TempData["UserError"] =
                    "This email is already in use";
                TempData["OpenCreateUserModal"] = true;
                return RedirectToAction(nameof(Index));
            }

            var user = new User
            {
                Username = normalizedUsername,
                Password = _passwordService.HashPassword(model.Password),
                FullName = model.FullName.Trim(),
                Email = normakizedEmail,
                Role = model.Role == "Admin" ? "Admin" : "User",
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["UserSuccess"] =
                "User created successfuly";

            return RedirectToAction(nameof(Index));
        }
    }
}
