using System.ComponentModel.DataAnnotations;

namespace deliveryApp.Models.Entities
{
    public class DishInCartEntity
    {
        public Guid Id { get; set; }
        [Required]
        public int Amount { get; set; }
        [Required]
        public UserEntity User { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public DishEntity Dish { get; set; }
        public OrderEntity? Order { get; set; }
    }
}
