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
            // อ่าน User Id จาก Cookie Authentication
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Challenge();
            }

            // ตรวจว่าสินค้ามีจริงและยังเปิดขายอยู่
            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == productId &&
                    p.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            // สินค้าหมด ไม่อนุญาตให้เพิ่มลง Cart
            if (product.Stock <= 0)
            {
                TempData["CartError"] = "This product is out of stock.";

                return RedirectToAction(nameof(Index));
            }

            // หา Cart ของ User
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            // ถ้ายังไม่มี Cart ให้สร้างใหม่
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Carts.Add(cart);
            }

            CartItem? cartItem = null;

            // Cart ใหม่ยังไม่มี Id จนกว่าจะ Save
            if (cart.Id != 0)
            {
                cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci =>
                        ci.CartId == cart.Id &&
                        ci.ProductId == productId);
            }

            if (cartItem == null)
            {
                // สินค้ายังไม่มีใน Cart
                cartItem = new CartItem
                {
                    Cart = cart,
                    ProductId = productId,
                    Quantity = 1
                };

                _context.CartItems.Add(cartItem);
            }
            else
            {
                // ป้องกันจำนวนใน Cart เกิน Stock
                if (cartItem.Quantity >= product.Stock)
                {
                    TempData["CartError"] =
                        "You cannot add more than the available stock.";

                    return RedirectToAction(nameof(Index));
                }

                cartItem.Quantity++;
            }

            await _context.SaveChangesAsync();

            TempData["CartSuccess"] =
                $"{product.Name} was added to your cart.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(userIdClaim, out int userId))
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