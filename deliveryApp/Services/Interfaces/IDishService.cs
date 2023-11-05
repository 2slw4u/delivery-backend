using deliveryApp.Models.DTOs;
using deliveryApp.Models.Enums;

namespace deliveryApp.Services.Interfaces
{
    public interface IDishService
    {
        Task<DishPagedListDto> GetMenu(DishCategory[] categories, string sorting, int page, bool vegetarian = false);
        Task<DishDto> GetDishInfo(Guid dishId);
        Task<bool> CheckIfUserCanSetRating(string token);
        Task<UserDto> GetProfile(string token);
        Task<Response> EditProfile(string token, UserEditModel newUserModel);
    }
}
