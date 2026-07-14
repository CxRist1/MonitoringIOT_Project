using Stripe;
using Stripe.Checkout;

namespace iot_monitoring.Services
{
    public class StripePaymentService
    {
        public async Task<PaymentIntent> CreatePromptPayPaymentAsync(
            decimal amount,
            string orderNumber)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100),
                Currency = "thb",

                PaymentMethodTypes = new List<string>
                {
                    "promptpay"
                },

                Metadata = new Dictionary<string, string>
                {
                    { "OrderNumber", orderNumber }
                }
            };

            var service = new PaymentIntentService();

            return await service.CreateAsync(options);
        }
    }
}