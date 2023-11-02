using System.ComponentModel.DataAnnotations;

namespace deliveryApp.Models.Entities
{
    public class TokenEntity
    {
        public Guid Id;
        [Required]
        public string Token { get; set; }
        [Required]
        public DateTime ExpirationDate { get; set; }
    }
}
