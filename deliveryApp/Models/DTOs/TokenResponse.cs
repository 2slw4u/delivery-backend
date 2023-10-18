using System.ComponentModel.DataAnnotations;

namespace deliveryApp.Models.DTOs
{
    public class TokenResponse
    {
        [Required]
        [MinLength(1)]
        public string token { get; set; }
    }
}
