﻿using deliveryApp.Models.DTOs;
using deliveryApp.Models.Enums;

namespace deliveryApp.Services.Interfaces
{
    public interface IDishService
    {
        Task<DishPagedListDto> GetMenu(DishCategory[] categories, DishSorting sorting, int page = 1, bool vegetarian = false);
        Task<DishDto> GetDishInfo(Guid dishId);
        Task<bool> CheckIfUserCanSetRating(HttpContext httpContext, Guid id);
        public Task SetRating(HttpContext httpContext, Guid dishId, int ratingScore);
        public Task AddDishToMenu(DishDto dishModel, DishCategory dishCategory);
        public Task ValidateDish(Guid dishId);
    }
}
