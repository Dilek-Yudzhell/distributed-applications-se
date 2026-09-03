using System.ComponentModel.DataAnnotations;

namespace FoodOrderingSystem.API.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int MenuItemId { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, 10000)]
        public decimal UnitPrice { get; set; }

        [MaxLength(250)]
        public string Notes { get; set; } = string.Empty;
    }
}