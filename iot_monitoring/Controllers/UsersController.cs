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
        public async Task<IActionResult> Index()
        {
            var users =await _context.Users
                .AsTracking()
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return View(users);
        }
    }
}
