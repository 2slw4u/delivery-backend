using System.ComponentModel.DataAnnotations;

namespace deliveryApp.Models.DTOs
{
    public class LoginCredentials
    {
        [EmailAddress]
        [Required]
        [MinLength(1)]
        public string Email { get; set; }
        [Required]
        [MinLength(1)]
        public string Password { get; set; }
    }
}
