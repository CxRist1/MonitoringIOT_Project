namespace iot_monitoring.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public decimal Amount { get; set; }

        public string Method { get; set; } = "PromptPay";

        public string Status { get; set; } = "Pending";

        public string? PaymentIntentId { get; set; }

        public string? TransactionReference { get; set; }

        public string? ClientSecret { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PaidAt { get; set; }

        public Order Order { get; set; } = null!;
    }
}