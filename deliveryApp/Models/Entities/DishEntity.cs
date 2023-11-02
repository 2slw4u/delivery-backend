using deliveryApp.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace deliveryApp.Models.Entities
{
    public class DishEntity
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public double Price { get; set; }
        public string? Description { get; set; }
        [Required]
        public Boolean IsVegetarian { get; set; }
        public string? Photo { get; set; }
        [Required]
        public DishCategory Category { get; set; }
        public List<RatingEntity> Ratings { get; set; }
    }
}
