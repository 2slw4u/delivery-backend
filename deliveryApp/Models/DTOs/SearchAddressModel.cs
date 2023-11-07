using deliveryApp.Models.Enums;

namespace deliveryApp.Models.DTOs
{
    public class SearchAddressModel
    {
        public long ObjectId { get; set; }
        public Guid ObjectGuid { get; set; }
        public string? Text { get; set; }
        public GarAddressLevel ObjectLevel { get; set; }
        public string? ObjectLevelText { get; set; }
    }
}
