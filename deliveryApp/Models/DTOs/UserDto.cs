using System.ComponentModel.DataAnnotations;
using deliveryApp.Models.Enums;

namespace deliveryApp.Models.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        [Required]
        [MinLength(1)]
        public string FullName { get; set; }
        public DateOnly? BirthDate { get; set; }
        [Required]
        public Gender Gender { get; set; }
        public Guid? Address { get; set; }
        [Required]
        [EmailAddress]
        [MinLength(1)]
        public string Email { get; set; }
        [Phone]
        public string? PhoneNumber { get; set; }
    }
}
