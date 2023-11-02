using System.ComponentModel.DataAnnotations;

namespace deliveryApp.Models.Entities
{
    public class DishInCartEntity
    {
        public Guid Id { get; set; }
        [Required]
        public int Count { get; set; }
        [Required]
        public DishEntity Dish { get; set; }
        [Required]
        public UserEntity User { get; set; }
    }
}
