using deliveryApp.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace deliveryApp.Models.DTOs
{
    public class UserRegisterModel
    {
        [Required]
        [MinLength(1)]
        public string fullName { get; set; }
        [Required]
        [MinLength(6)]
        public string password { get; set; }
        [Required]
        [MinLength(1)]
        [EmailAddress]
        public string Email { get; set; }
        public Guid? addressId { get; set; }
        public DateTime? birthDate { get; set; }
        [Required]
        public Gender gender { get; set; }
        [Phone]
        public string? phoneNumber { get; set; }
    }
}
