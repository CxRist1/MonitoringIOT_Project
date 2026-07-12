using iot_monitoring.Data;
using iot_monitoring.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace iot_monitoring.Controllers
{
    [Authorize]
    public class ShopController : Controller
    {
        private readonly AppDbContext _context;

        public ShopController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Challenge();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == productId &&
                    p.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            if (product.Stock <= 0)
            {
                TempData["CartError"] =
                    "Product is out of stock.";

                return RedirectToAction(nameof(Index));
            }

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Carts.Add(cart);

                await _context.SaveChangesAsync();
            }

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci =>
                    ci.CartId == cart.Id &&
                    ci.ProductId == productId);

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = 1
                };

                _context.CartItems.Add(cartItem);
            }
            else
            {
                if (cartItem.Quantity >= product.Stock)
                {
                    TempData["CartError"] =
                        "Quantity cannot exceed available stock.";

                    return RedirectToAction(nameof(Index));
                }

                cartItem.Quantity++;
            }

            await _context.SaveChangesAsync();

            TempData["CartSuccess"] =
                $"{product.Name} added to your cart.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Challenge();
            }

            var cart = await _context.Carts
                .AsNoTracking()
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return View(cart);
        }
    }
}