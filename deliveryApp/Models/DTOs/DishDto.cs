using deliveryApp.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace deliveryApp.Models.DTOs
{
    public class DishDto
    {
        public Guid id { get; set; }
        [Required]
        [MinLength(1)]
        public string name { get; set; }
        public string? description { get; set; }
        [Required]
        public double price { get; set; }
        public Boolean vegetarian { get; set; }
        public double? rating { get; set; }
        public DishCategory category;
    }
}
