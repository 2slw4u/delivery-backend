using deliveryApp.Models.DTOs;

namespace deliveryApp.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> GetOrderInfo(HttpContext httpContext, Guid orderId);
        Task<List<OrderDto>> GetAllOrders(HttpContext httpContext);
        Task CreateOrderFromCurrentBasket(HttpContext httpContext, OrderCreateDto newOrder);
        Task ConfirmOrderDelivery(HttpContext httpContext, Guid orderId);
    }
}
