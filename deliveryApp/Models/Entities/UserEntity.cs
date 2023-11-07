using deliveryApp.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace deliveryApp.Models.Entities
{
    public class UserEntity
    {
        public Guid Id { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Password { get; set; }
        public DateTime? BirthDate { get; set; }
        [Required]
        public Gender Gender { get; set; }
        [Phone]
        public string? Phone { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public Guid? AddressGuid { get; set; }
    }
}
