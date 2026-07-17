using iot_monitoring.Data;
using iot_monitoring.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace iot_monitoring.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/stripe/webhook")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeWebhookController> _logger;
        private readonly ILineMessagingService _lineMessagingService;

        public StripeWebhookController(
            AppDbContext context,
            IConfiguration configuration,
            ILogger<StripeWebhookController> logger,
            ILineMessagingService lineMessagingService)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _lineMessagingService = lineMessagingService;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(
                HttpContext.Request.Body
            ).ReadToEndAsync();

            var webhookSecret =
                _configuration["Stripe:WebhookSecret"];

            if (string.IsNullOrWhiteSpace(webhookSecret))
            {
                _logger.LogError(
                    "Stripe WebhookSecret is not configured."
                );

                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret
                );
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Invalid Stripe webhook signature."
                );

                return BadRequest();
            }

            if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
            {
                var paymentIntent =
                    stripeEvent.Data.Object as PaymentIntent;

                if (paymentIntent == null)
                {
                    return BadRequest();
                }

                await HandlePaymentSucceededAsync(paymentIntent);
            }
            else if (
                stripeEvent.Type ==
                EventTypes.PaymentIntentPaymentFailed)
            {
                var paymentIntent =
                    stripeEvent.Data.Object as PaymentIntent;

                if (paymentIntent != null)
                {
                    await HandlePaymentFailedAsync(paymentIntent);
                }
            }

            return Ok();
        }

        private async Task HandlePaymentSucceededAsync(
            PaymentIntent paymentIntent)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(p =>
                    p.PaymentIntentId == paymentIntent.Id);

            if (payment == null)
            {
                _logger.LogWarning(
                    "Payment not found for PaymentIntent {PaymentIntentId}.",
                    paymentIntent.Id
                );

                return;
            }

            if (payment.Status == "Paid")
            {
                if (!payment.PaidAt.HasValue)
                {
                    payment.PaidAt = DateTime.UtcNow;
                    payment.TransactionReference ??= paymentIntent.Id;

                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        "Repaired missing PaidAt for Payment {PaymentId}.",
                        payment.Id);
                }
                return;
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var orderItem in payment.Order.OrderItems)
                {
                    if (orderItem.Quantity > orderItem.Product.Stock)
                    {
                        throw new InvalidOperationException(
                            $"Not enough stock for {orderItem.Product.Name}."
                        );
                    }

                    orderItem.Product.Stock -= orderItem.Quantity;
                }

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c =>
                        c.UserId == payment.Order.UserId);

                if (cart != null)
                {
                    _context.CartItems.RemoveRange(cart.CartItems);
                }

                payment.Status = "Paid";
                payment.PaidAt = DateTime.UtcNow;
                payment.TransactionReference = paymentIntent.Id;

                payment.Order.Status = "Completed";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Order {OrderId} completed successfully.",
                    payment.OrderId
                );
                try
                {
                    var paidAtUtc = payment.PaidAt ?? DateTime.UtcNow;

                    var thailandTimeZone =
                        TimeZoneInfo.FindSystemTimeZoneById(
                            "SE Asia Standard Time");

                    var paidAtThailand =
                        TimeZoneInfo.ConvertTimeFromUtc(
                            DateTime.SpecifyKind(
                                paidAtUtc,
                                DateTimeKind.Utc),
                            thailandTimeZone);

                    var message =
$"""
✅ ชำระเงินสำเร็จ

Order: #{payment.OrderId}
ยอดรวม: ฿{payment.Amount:N2}

👤 ชื่อผู้รับ: {payment.Order.RecipientName}
📞 เบอร์โทร: {payment.Order.PhoneNumber}
📍 ที่อยู่จัดส่ง:
{payment.Order.ShippingAddress}

สถานะ Order: Completed
สถานะการชำระ: ชำระแล้ว
ช่องทาง: {payment.Method}

เวลา: {paidAtThailand:dd/MM/yyyy HH:mm}

พร้อมดำเนินการจัดส่ง 🚚
""";
                    await _lineMessagingService.SendMessageAsync(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "order {OrderId} completed, but LINE payment notification failed.",
                        payment.OrderId
                        );
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Failed to complete Order {OrderId}.",
                    payment.OrderId
                );

                throw;
            }
        }

        private async Task HandlePaymentFailedAsync(
            PaymentIntent paymentIntent)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p =>
                    p.PaymentIntentId == paymentIntent.Id);

            if (payment == null)
            {
                return;
            }

            payment.Status = "Failed";
            payment.Order.Status = "PaymentFailed";

            await _context.SaveChangesAsync();
        }
    }
}