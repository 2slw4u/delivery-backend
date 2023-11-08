using deliveryApp.Models.DTOs;

namespace deliveryApp.Services.Interfaces
{
    public interface IBasketService
    {
        Task<List<DishBasketDto>> Get(HttpContext httpContext);
        Task AddDish(HttpContext httpContext, Guid dishId);
        Task RemoveDish(HttpContext httpContext, Guid dishId, bool increase = false);
    }
}
