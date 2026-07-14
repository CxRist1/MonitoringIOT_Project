using Stripe;

namespace iot_monitoring.Services
{
    public class StripePaymentService
    {
        public async Task<PaymentIntent> CreatePromptPayPaymentAsync(
            int orderId,
            decimal amount,
            string customerEmail)
        {
            var amountInSatang = decimal.ToInt64(
                decimal.Round(
                    amount * 100,
                    0,
                    MidpointRounding.AwayFromZero
                )
            );

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInSatang,
                Currency = "thb",
                ReceiptEmail = customerEmail,

                PaymentMethodTypes = new List<string>
                {
                    "promptpay"
                },

                Metadata = new Dictionary<string, string>
                {
                    { "OrderId", orderId.ToString() }
                }
            };

            var service = new PaymentIntentService();

            return await service.CreateAsync(options);
        }
    }
}