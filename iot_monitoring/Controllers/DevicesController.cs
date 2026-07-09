using iot_monitoring.Data;
using iot_monitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iot_monitoring.Controllers
{
	public class DevicesController : Controller
	{
		private readonly AppDbContext _context;

		public DevicesController(AppDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index()
		{
			var devices = await _context.Devices.ToListAsync();
			return View(devices);
		}

		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(Device device)
		{
			if (!ModelState.IsValid)
			{
				return View(device);
			}

			_context.Devices.Add(device);
			await _context.SaveChangesAsync();

			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> Delete(int id)
		{
			var device = await _context.Devices.FindAsync(id);

			if (device == null)
			{
				return NotFound();
			}

			_context.Devices.Remove(device);
			await _context.SaveChangesAsync();

			return RedirectToAction("Index");
		}
	}
}