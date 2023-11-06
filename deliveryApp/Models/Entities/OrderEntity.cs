using deliveryApp.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace deliveryApp.Models.Entities
{
    public class OrderEntity
    {
        public Guid Id { get; set; }
        [Required]
        public DateTime DeliveryTime { get; set; }
        [Required]
        public DateTime OrderTime { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public string AddresId { get; set; }
        [Required]
        public OrderStatus Status { get; set; }
        [Required]
        public UserEntity User { get; set; }
        [Required]
        public string Address { get; set; }

    }
}
