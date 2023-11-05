using deliveryApp.Models.DTOs;

namespace deliveryApp.Services.Interfaces
{
    public interface IBasketService
    {
        Task<List<DishBasketDto>> Get(string token);
        Task AddDish(string token, Guid dishId);
        Task RemoveDish(string token, Guid dishId, bool increase = false);
    }
}
