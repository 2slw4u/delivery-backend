using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace deliveryApp.Models.DTOs
{
    public class DishBasketDto
    {
        public Guid Id { get; set; }
        [Required]
        [MinLength(1)]
        public string name { get; set; }
        [Required]
        public double price { get; set; }
        [Required]
        public double totalPrice { get; set; }
        [Required]
        public int amount { get; set; }
        public string? image { get; set; }
    }
}
