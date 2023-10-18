using System.ComponentModel.DataAnnotations;
using deliveryApp.Models.Enums;

namespace deliveryApp.Models.DTOs
{
    public class UserDto
    {
        public Guid id { get; set; }
        [Required]
        [MinLength(1)]
        public string fullName { get; set; }
        public DateOnly? birthDate { get; set; }
        [Required]
        public Gender gender { get; set; }
        public Guid? address { get; set; }
        [Required]
        [EmailAddress]
        [MinLength(1)]
        public string email { get; set; }
        [Phone]
        public string? phoneNumber { get; set; }
    }
}
