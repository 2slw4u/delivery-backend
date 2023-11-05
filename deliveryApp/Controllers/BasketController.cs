using deliveryApp.Models.DTOs;
using deliveryApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace deliveryApp.Controllers
{
    [ApiController]
    [Route("api/basket")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketService _basketService;
        public BasketController (IBasketService basketService)
        {
            _basketService = basketService;
        }
        [HttpGet]
        public async Task<List<DishBasketDto>> GetBasket(string token)
        {
            return await _basketService.Get(token);
        }
        [HttpPost]
        [Route("/dish/{dishId}")]
        public async Task AddDish(string token, Guid dishId)
        {
            await _basketService.AddDish(token, dishId);
        }
    }
}
