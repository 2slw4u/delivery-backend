using deliveryApp.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace deliveryApp.Models.DTOs
{
    public class OrderInfoDto
    {
        public Guid Id { get; set; }
        [Required]
        public DateTime DeliveryTime { get; set; }
        [Required]
        public DateTime OrderTime { get; set; }
        [Required]
        public OrderStatus Status { get; set; }
        [Required]
        public double Price { get; set; }
    }
}
