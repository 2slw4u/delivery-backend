using deliveryApp.Models.Enums;

namespace deliveryApp.Models.DTOs
{
    public class SearchAddressModel
    {
        public int objectId { get; set; }
        public Guid objectGuid { get; set; }
        public string? text { get; set; }
        public GarAddressLevel objectLevel { get; set; }
        public string? objectLevelText { get; set; }
    }
}
