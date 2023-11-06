using deliveryApp.Models.DTOs;

namespace deliveryApp.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> GetOrderInfo(string token, Guid orderId);
        Task<List<OrderDto>> GetAllOrders(string token);
        Task CreateOrderFromCurrentBasket(string token, OrderCreateDto newOrder);
        Task ConfirmOrderDelivery(string token, Guid orderId);
    }
}
