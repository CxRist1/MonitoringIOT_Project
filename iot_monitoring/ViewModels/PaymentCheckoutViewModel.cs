namespace iot_monitoring.ViewModels
{
    public class PaymentCheckoutViewModel
    {
        public int OrderId { get; set; }

        public decimal Amount { get; set; }

        public string ClientSecret { get; set; } = string.Empty;

        public string PublishableKey { get; set; } = string.Empty;

        public string CustomerEmail { get; set; } = string.Empty;
    }
}