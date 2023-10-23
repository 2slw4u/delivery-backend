using deliveryApp.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace deliveryApp.Models.DTOs
{
    public class DishDto
    {
        public Guid Id { get; set; }
        [Required]
        [MinLength(1)]
        public string Name { get; set; }
        public string? Description { get; set; }
        [Required]
        public double Price { get; set; }
        public Boolean Vegetarian { get; set; }
        public double? Rating { get; set; }
        public DishCategory Category;
    }
}
