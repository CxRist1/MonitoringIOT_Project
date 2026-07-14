using iot_monitoring.Data;
using iot_monitoring.Models;
using iot_monitoring.Services;
using iot_monitoring.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace iot_monitoring.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly StripePaymentService _stripePaymentService;
        private readonly IConfiguration _configuration;

        public PaymentController(
            AppDbContext context,
            StripePaymentService stripePaymentService,
            IConfiguration configuration)
        {
            _context = context;
            _stripePaymentService = stripePaymentService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(int orderId)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out int userId))
            {
                return Challenge();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o =>
                    o.Id == orderId &&
                    o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != "PendingPayment")
            {
                TempData["CartError"] =
                    "This order is not waiting for payment.";

                return RedirectToAction(
                    "OrderHistory",
                    "Shop"
                );
            }

            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == order.Id);

            Payment payment;

            if (existingPayment == null)
            {
                var paymentIntent =
                    await _stripePaymentService
                        .CreatePromptPayPaymentAsync(
                            order.Id,
                            order.TotalAmount,
                            order.User.Email
                        );

                payment = new Payment
                {
                    OrderId = order.Id,
                    Amount = order.TotalAmount,
                    Method = "PromptPay",
                    Status = "Pending",
                    PaymentIntentId = paymentIntent.Id,
                    ClientSecret = paymentIntent.ClientSecret,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
            }
            else
            {
                payment = existingPayment;
            }

            var publishableKey =
                _configuration["Stripe:PublishableKey"];

            if (string.IsNullOrWhiteSpace(publishableKey))
            {
                throw new InvalidOperationException(
                    "Stripe PublishableKey is not configured."
                );
            }

            var model = new PaymentCheckoutViewModel
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,
                ClientSecret = payment.ClientSecret
                    ?? throw new InvalidOperationException(
                        "Payment ClientSecret is missing."
                    ),
                PublishableKey = publishableKey,
                CustomerEmail = order.User.Email
            };

            return View(model);
        }
    }
}