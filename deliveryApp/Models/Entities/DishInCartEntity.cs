using System.ComponentModel.DataAnnotations;

namespace deliveryApp.Models.Entities
{
    public class DishInCartEntity
    {
        public Guid Id { get; set; }
        [Required]
        public int Amount { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public DishEntity Dish { get; set; }
        [Required]
        public OrderEntity Order { get; set; }
    }
}
