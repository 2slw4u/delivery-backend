using System.ComponentModel.DataAnnotations;

namespace deliveryApp.Models.DTOs
{
    public class OrderCreateDto
    {
        [Required]
        public DateTime DeliveryTime { get; set; }
        [Required]
        public Guid AddressId { get; set; }
    }
}
