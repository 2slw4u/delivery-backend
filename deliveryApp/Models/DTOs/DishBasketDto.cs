using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace deliveryApp.Models.DTOs
{
    public class DishBasketDto
    {
        public Guid Id { get; set; }
        [Required]
        [MinLength(1)]
        public string Name { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public double TotalPrice { get; set; }
        [Required]
        public int Amount { get; set; }
        public string? Image { get; set; }
    }
}
