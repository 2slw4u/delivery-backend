using deliveryApp.Models.DTOs;
using deliveryApp.Models.Enums;
using deliveryApp.Services.Interfaces;

namespace deliveryApp.Services
{
    public class DishService : IDishService
    {
        public Task<bool> CheckIfUserCanSetRating(string token, Guid dishId)
        {
            throw new NotImplementedException();
        }

        public Task<DishDto> GetDishInfo(Guid dishId)
        {
            throw new NotImplementedException();
        }

        public Task<DishPagedListDto> GetMenu(DishCategory[] categories, string sorting, int page, bool vegetarian = false)
        {
            throw new NotImplementedException();
        }

        public Task SetRating(string token, Guid dishId, int ratingScore)
        {
            throw new NotImplementedException();
        }
    }
}
