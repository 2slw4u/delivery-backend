using deliveryApp.Models.DTOs;
using deliveryApp.Services.Interfaces;
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
        [Route("/{orderId}")]
        public async Task<OrderDto> GetOrderInfo(string token, Guid orderId)
        {
            return await _orderService.GetOrderInfo(token, orderId);
        }
    }
}
