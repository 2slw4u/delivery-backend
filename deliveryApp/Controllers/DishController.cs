using deliveryApp.Models.DTOs;
using deliveryApp.Models.Enums;
using deliveryApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace deliveryApp.Controllers
{
    [ApiController]
    [Route("api/dish")]
    public class DishController : ControllerBase
    {
        private readonly IDishService _dishService;
        public DishController(IDishService dishService)
        {
            _dishService = dishService;
        }
        [HttpPost]
        [Route("addToMenu")]
        public async Task AddToMenu(DishDto newDish, DishCategory dishCategory)
        {
            await _dishService.AddDishToMenu(newDish, dishCategory);
        }
        [HttpGet]
        public async Task<DishPagedListDto> GetMenu([FromQuery]DishCategory[] categories, DishSorting sorting, int page = 1, bool vegetarian = false)
        {
            return await _dishService.GetMenu(categories, sorting, page, vegetarian);
        }

        [HttpGet]
        [Route("{dishId}")]
        public async Task<DishDto> GetDishInfo(Guid dishId)
        {
            return await _dishService.GetDishInfo(dishId);
        }
        [HttpGet]
        [Route("{dishId}/rating/check")]
        [Authorize(Policy = "AuthorizationPolicy")]
        public async Task<bool> CheckIfUserCanSetRating(Guid dishId)
        {
            return await _dishService.CheckIfUserCanSetRating(HttpContext, dishId);
        }
        [HttpPost]
        [Route("{dishId}/rating")]
        [Authorize(Policy = "AuthorizationPolicy")]
        public async Task SetRating(Guid dishId, int ratingScore)
        {
            await _dishService.SetRating(HttpContext, dishId, ratingScore);
        }
    }
}
