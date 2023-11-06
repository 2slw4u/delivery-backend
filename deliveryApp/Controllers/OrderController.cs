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
        [Route("{orderId}")]
        public async Task<OrderDto> GetOrderInfo(string token, Guid orderId)
        {
            return await _orderService.GetOrderInfo(token, orderId);
        }
        [HttpGet]
        public async Task<List<OrderDto>> GetAllOrders(string token)
        {
            return await _orderService.GetAllOrders(token);
        }
        [HttpPost]
        [Route("{orderId}/status")]
        public async Task ConfirmOrderDelivery(string token, Guid orderId)
        {
            await _orderService.ConfirmOrderDelivery(token, orderId);
        }
    }
}
