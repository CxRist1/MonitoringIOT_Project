using iot_monitoring.Data;
using iot_monitoring.Services;
using iot_monitoring.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using iot_monitoring.ViewModels;

namespace iot_monitoring.Controllers
{
    [Authorize]
    public class ShopController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILineMessagingService _lineMessagingService;
        private readonly ILogger<ShopController> _logger;

        public ShopController(
            AppDbContext context,
            ILineMessagingService lineMessagingService,
            ILogger<ShopController> logger)
        {
            _context = context;
            _lineMessagingService = lineMessagingService;
            _logger = logger;
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
        [HttpGet]
        public async Task<IActionResult> OrderSuccess(int orderId)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Challenge();
            }

            var orders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o =>
                    o.Id == orderId &&
                    o.UserId == userId);

            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPurchase(
            ConfirmPurchaseViewModel model)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Challenge();
            }
            if (!ModelState.IsValid)
            {
                TempData["CartError"] =
                    "Please complete the shipping information.";

                return RedirectToAction(nameof(Cart));
            }
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["CartError"] = "Your cart is empty.";

                return RedirectToAction(nameof(Cart));
            }

            foreach (var item in cart.CartItems)
            {
                if (!item.Product.IsActive)
                {
                    TempData["CartError"] =
                        $"{item.Product.Name} is no longer available.";

                    return RedirectToAction(nameof(Cart));
                }

                if (item.Quantity > item.Product.Stock)
                {
                    TempData["CartError"] =
                        $"Not enough stock for {item.Product.Name}.";

                    return RedirectToAction(nameof(Cart));
                }
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var order = new Order
                {
                    UserId = userId,
                    Status = "PendingPayment",
                    CreatedAt = DateTime.UtcNow,

                    RecipientName = model.RecipientName.Trim(),
                    PhoneNumber = model.PhoneNumber.Trim(),
                    ShippingAddress = model.ShippingAddress.Trim()
                };

                foreach (var cartItem in cart.CartItems)
                {
                    var subtotal =
                        cartItem.Product.Price * cartItem.Quantity;

                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.Product.Price,
                        Subtotal = subtotal
                    });
                }

                order.TotalAmount = order.OrderItems
                    .Sum(item => item.Subtotal);

                _context.Orders.Add(order);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                try
                {
                    var thailandTimeZone =
                        TimeZoneInfo.FindSystemTimeZoneById(
                            "SE Asia Standard Time");

                    var createdAtThailand =
                        TimeZoneInfo.ConvertTimeFromUtc(
                            order.CreatedAt,
                            thailandTimeZone);

                    var message =
        $"""
🛒 มีคำสั่งซื้อใหม่

Order: #{order.Id}
ยอดรวม: ฿{order.TotalAmount:N2}

สถานะ Order: PendingPayment
สถานะการชำระ: ⏳ ยังไม่ได้ชำระเงิน

เวลา: {createdAtThailand:dd/MM/yyyy HH:mm}
""";

                    await _lineMessagingService.SendMessageAsync(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Order {OrderId} was created, but LINE notification failed.",
                        order.Id);
                }

                return RedirectToAction(
                    "Checkout",
                    "Payment",
                    new { orderId = order.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Failed to create order for User {UserId}.",
                    userId);

                TempData["CartError"] =
                    "Unable to create the order. Please try again.";

                return RedirectToAction(nameof(Cart));
            }
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
        public async Task<IActionResult> OrderHistory()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Challenge();
            }
            var order = await _context.Orders
                        .AsNoTracking()
                        .Where(o => o.UserId == userId)
                        .OrderByDescending(o => o.CreatedAt)
                        .ToListAsync();
            return View(order);
        }
        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Challenge();
            }

            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o =>
                    o.Id == id &&
                    o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IncreaseQuantity(int cartItemId)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Challenge();
            }

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci =>
                    ci.Id == cartItemId &&
                    ci.Cart.UserId == userId);

            if (cartItem == null)
            {
                return NotFound();
            }

            if (cartItem.Quantity >= cartItem.Product.Stock)
            {
                TempData["CartError"] =
                    "Quantity cannot exceed available stock.";

                return RedirectToAction(nameof(Cart));
            }

            cartItem.Quantity++;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DecreaseQuantity(int cartItemId)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Challenge();
            }

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci =>
                    ci.Id == cartItemId &&
                    ci.Cart.UserId == userId);

            if (cartItem == null)
            {
                return NotFound();
            }

            if (cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
            }
            else
            {
                _context.CartItems.Remove(cartItem);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Challenge();
            }

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci =>
                    ci.Id == cartItemId &&
                    ci.Cart.UserId == userId);

            if (cartItem == null)
            {
                return NotFound();
            }

            _context.CartItems.Remove(cartItem);

            await _context.SaveChangesAsync();
            TempData["CartSuccess"] =
                "Product removed from your cart.";

            return RedirectToAction(nameof(Cart));
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