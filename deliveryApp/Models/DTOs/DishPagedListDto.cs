namespace deliveryApp.Models.DTOs
{
    public class DishPagedListDto
    {
        public List<DishDto>? dishes { get; set; }
        public PageInfoModel pagination { get; set; }
    }
}
