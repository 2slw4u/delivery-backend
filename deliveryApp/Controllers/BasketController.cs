using deliveryApp.Models.DTOs;
using deliveryApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Policy = "AuthorizationPolicy")]
        public async Task<List<DishBasketDto>> GetBasket()
        {
            return await _basketService.Get(HttpContext);
        }
        [HttpPost]
        [Route("dish/{dishId}")]
        [Authorize(Policy = "AuthorizationPolicy")]
        public async Task AddDish(Guid dishId)
        {
            await _basketService.AddDish(HttpContext, dishId);
        }
        [HttpDelete]
        [Route("dish/{dishId}")]
        [Authorize(Policy = "AuthorizationPolicy")]
        public async Task RemoveDish(Guid dishId, bool increase=false)
        {
            await _basketService.RemoveDish(HttpContext, dishId, increase);
        }
    }
}
