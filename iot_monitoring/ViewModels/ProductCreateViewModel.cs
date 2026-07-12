using System.ComponentModel.DataAnnotations;

namespace iot_monitoring.ViewModels
{
    public class ProductCreateViewModel
    {
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0.01, 9999999, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int Stock { get; set; }

        [StringLength(300)]
        public string? ImageUrl { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        public bool IsActive { get; set; } = true;
    }
}