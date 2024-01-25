using System.ComponentModel.DataAnnotations;

namespace deliveryApp.Models.Entities
{
    public class RatingEntity
    {
        public Guid Id { get; set; }
        [Required]
        [Range(0, 10)]
        public int Value { get; set; }
        [Required]
        public DishEntity Dish { get; set; }
        [Required]
        public UserEntity User { get; set; }
    }
}
