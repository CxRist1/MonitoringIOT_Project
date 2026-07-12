using iot_monitoring.Data;
using iot_monitoring.Models;
using iot_monitoring.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace iot_monitoring.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ProductCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var product = new Product
            {
                Name = model.Name.Trim(),
                Description = model.Description?.Trim(),
                Price = model.Price,
                Stock = model.Stock,
                ImageUrl = model.ImageUrl?.Trim(),
                Category = model.Category?.Trim(),
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product created successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}