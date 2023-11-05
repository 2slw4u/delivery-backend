using deliveryApp.Models.DTOs;
using deliveryApp.Services.Interfaces;
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
        [HttpGet]
        [Route("/{dishId}")]
        public async Task<DishDto> GetDishInfo(Guid dishId)
        {
            return await _dishService.GetDishInfo(dishId);
        }
    }
}
