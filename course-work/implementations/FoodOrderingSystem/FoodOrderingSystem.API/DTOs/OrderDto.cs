using System.ComponentModel.DataAnnotations;

namespace FoodOrderingSystem.API.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(0.01, 100000)]
        public decimal TotalPrice { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Pending";

        [Required]
        [MaxLength(250)]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required]
        public DateTime OrderDate { get; set; }
    }
}