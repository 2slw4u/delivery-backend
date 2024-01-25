namespace deliveryApp.Models.DTOs
{
    public class DishPagedListDto
    {
        public List<DishDto>? Dishes { get; set; }
        public PageInfoModel Pagination { get; set; }
    }
}
