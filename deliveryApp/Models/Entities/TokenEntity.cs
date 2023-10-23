using System.ComponentModel.DataAnnotations;

namespace deliveryApp.Models.Entities
{
    public class TokenEntity
    {
        public Guid id;
        [Required]
        public string token { get; set; }
        [Required]
        public DateTime ExpirationDate { get; set; }
    }
}
