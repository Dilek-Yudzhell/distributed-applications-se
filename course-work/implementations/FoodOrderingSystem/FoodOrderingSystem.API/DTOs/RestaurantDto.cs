using System.ComponentModel.DataAnnotations;

namespace FoodOrderingSystem.API.DTOs
{
    public class RestaurantDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string CuisineType { get; set; } = string.Empty;

        [Range(0, 5)]
        public decimal Rating { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsActive { get; set; }
    }
}