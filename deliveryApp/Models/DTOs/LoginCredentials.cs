using System.ComponentModel.DataAnnotations;

namespace deliveryApp.Models.DTOs
{
    public class LoginCredentials
    {
        [Required]
        [MinLength(1)]
        public string email { get; set; }
        [Required]
        [MinLength(1)]
        public string password { get; set; }
    }
}
