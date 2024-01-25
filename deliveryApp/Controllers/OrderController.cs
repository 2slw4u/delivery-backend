using deliveryApp.Models.DTOs;
using deliveryApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace deliveryApp.Controllers
{
    [ApiController]
    [Route("api/order")]
    public class OrderController : ControllerBase 
    {
        private readonly IOrderService _orderService;
        
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [HttpGet]
        [Route("{orderId}")]
        [Authorize(Policy = "AuthorizationPolicy")]
        public async Task<OrderDto> GetOrderInfo(Guid orderId)
        {
            return await _orderService.GetOrderInfo(HttpContext, orderId);
        }
        [HttpGet]
        [Authorize(Policy = "AuthorizationPolicy")]
        public async Task<List<OrderDto>> GetAllOrders()
        {
            return await _orderService.GetAllOrders(HttpContext);
        }
        [HttpPost]
        [Authorize(Policy = "AuthorizationPolicy")]
        public async Task CreateOrderFromBasket(OrderCreateDto newOrder)
        {
            await _orderService.CreateOrderFromCurrentBasket(HttpContext, newOrder);
        }
        [HttpPost]
        [Route("{orderId}/status")]
        [Authorize(Policy = "AuthorizationPolicy")]
        public async Task ConfirmOrderDelivery(Guid orderId)
        {
            await _orderService.ConfirmOrderDelivery(HttpContext, orderId);
        }

    }
}
