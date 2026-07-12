using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
namespace iot_monitoring.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)] 
        public string? Description { get; set;}

        [Column(TypeName = "numberic(10,2)")]
        public decimal Price { get; set; }

        public int Stock {  get; set; }

        [StringLength(300)]
        public string? ImageUrl { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt {  get; set; } = DateTime.UtcNow;
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}


