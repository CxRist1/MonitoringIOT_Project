using iot_monitoring.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iot_monitoring.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;
        public UsersController(AppDbContext context)
        {
            _context = context;
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
    }
}
