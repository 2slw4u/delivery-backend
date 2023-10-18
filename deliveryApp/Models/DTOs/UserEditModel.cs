using deliveryApp.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace deliveryApp.Models.DTOs
{
    public class UserEditModel
    {
        [Required]
        [MinLength(1)]
        public string fullName { get; set; }
        public DateTime? birthDate { get; set; }
        [Required]
        public Gender gender { get; set; }
        public Guid? addressId { get; set; }
        [Phone]
        public string? phoneNumber { get; set; }
    }
}
