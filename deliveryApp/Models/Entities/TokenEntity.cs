using System.ComponentModel.DataAnnotations;

namespace deliveryApp.Models.Entities
{
    public class TokenEntity
    {
        public Guid Id { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        [EmailAddress]
        public string userEmail { get; set; }
        [Required]
        public DateTime ExpirationDate { get; set; }
    }
}
