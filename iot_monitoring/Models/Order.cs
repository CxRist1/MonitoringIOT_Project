using System.ComponentModel.DataAnnotations;

namespace iot_monitoring.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "PendingPayment";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(150)]
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string ShippingAddress {  get; set; } = string.Empty;

        public User User { get; set; } = null!;

        public ICollection<OrderItem> OrderItems { get; set; }
            = new List<OrderItem>();
    }
}