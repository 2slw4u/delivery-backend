using deliveryApp.Models.DTOs;

namespace deliveryApp.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> GetOrderInfo(string token, Guid orderId);
        Task<List<OrderDto>> GetAllOrders(string token);
        Task<OrderCreateDto> CreateOrderFromCurrentBasket(string token, DateTime deliveryTime, Guid addresId);
        Task ConfirmOrderDelivery(string token, Guid orderId);
    }
}
