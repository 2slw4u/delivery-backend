using deliveryApp.Models;
using deliveryApp.Models.DTOs;
using deliveryApp.Models.Entities;
using deliveryApp.Models.Enums;
using deliveryApp.Models.Exceptions;
using deliveryApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace deliveryApp.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IAddressService _addressService;
        const int ORDER_AND_DELIVERY_DIFFERENCE = 60;

        public OrderService(AppDbContext context, IAddressService addressService)
        {
            _context = context;
            _addressService = addressService;
        }
        public async Task ConfirmOrderDelivery(string token, Guid orderId)
        {
            try
            {
                await ValidateToken(token);
                await ValidateOrder(token, orderId);
                var tokenInDB = await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync();
                var orderOfAUserEntity = await _context.Orders.Where(x => x.Id == orderId && x.User.Email == tokenInDB.userEmail).FirstOrDefaultAsync();
                if (orderOfAUserEntity.Status == OrderStatus.Delievered)
                {
                    throw new Conflict("Selected order has already confirmed delivery");
                }
                orderOfAUserEntity.Status = OrderStatus.Delievered;
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new BadHttpRequestException(e.Message);
            }
        }

        public async Task CreateOrderFromCurrentBasket(string token, OrderCreateDto newOrder)
        {
            try
            {
                await ValidateToken(token);
                var tokenInDB = await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync();
                var dishesInOrder = await _context.DishesInCart.Where(x => x.User.Email == tokenInDB.userEmail && x.Order == null).ToListAsync();
                if (dishesInOrder.Count == 0)
                {
                    throw new BadRequest("There is no dishes in users current basket");
                }
                if ((newOrder.DeliveryTime - DateTime.Now.ToUniversalTime()).TotalMinutes < ORDER_AND_DELIVERY_DIFFERENCE)
                {
                    throw new Forbidden("Delivery time must be at least an hour after order time");
                }
                var totalPrice = 0.0;
                foreach (var dish in dishesInOrder)
                {
                    totalPrice += (dish.Price * dish.Amount);
                }
                await _addressService.ValidateAddressGuid(newOrder.AddressId);
                var result = new OrderEntity()
                {
                    Id = Guid.NewGuid(),
                    DeliveryTime = newOrder.DeliveryTime.ToUniversalTime(),
                    OrderTime = DateTime.Now.ToUniversalTime(),
                    Price = totalPrice,
                    AddresGuid = newOrder.AddressId,
                    Status = OrderStatus.InProcess,
                    User = await _context.Users.Where(x => x.Email == tokenInDB.userEmail).FirstOrDefaultAsync()
                };
                await _context.Orders.AddAsync(result);
                foreach (var dish in dishesInOrder)
                {
                    dish.Order = result;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new BadHttpRequestException(e.Message);
            }
        }

        public async Task<List<OrderDto>> GetAllOrders(string token)
        {
            try
            {
                await ValidateToken(token);
                var tokenInDB = await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync();
                var allOrders = await _context.Orders.Where(x => x.User.Email == tokenInDB.userEmail).ToListAsync();
                var result = new List<OrderDto>();
                foreach (var order in allOrders)
                {
                    result.Add(await GetOrderInfo(token, order.Id));
                }
                return result;
            }
            catch (Exception e)
            {
                throw new BadHttpRequestException(e.Message);
            }
        }

        public async Task<OrderDto> GetOrderInfo(string token, Guid orderId)
        {
            try
            {
                await ValidateToken(token);
                await ValidateOrder(token, orderId);
                var tokenInDB = await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync();
                var orderOfAUserEntity = await _context.Orders.Where(x => x.Id == orderId && x.User.Email == tokenInDB.userEmail).FirstOrDefaultAsync();
                var currentDishBasketDtoList = new List<DishBasketDto>();
                var dishesInOrder = await _context.DishesInCart.Where(x => x.Order == orderOfAUserEntity).ToListAsync();
                foreach (var DishInCart in dishesInOrder)
                {
                    currentDishBasketDtoList.Add(new DishBasketDto()
                    {
                        Id = DishInCart.Id,
                        Name = DishInCart.Dish.Name,
                        Price = DishInCart.Price,
                        TotalPrice = DishInCart.Price * DishInCart.Amount,
                        Amount = DishInCart.Amount,
                        Image = DishInCart.Dish.Photo
                    });
                }
                var result = new OrderDto()
                {
                    Id = orderId,
                    DeliveryTime = orderOfAUserEntity.DeliveryTime.ToUniversalTime(),
                    OrderTime = orderOfAUserEntity.OrderTime.ToUniversalTime(),
                    Status = orderOfAUserEntity.Status,
                    Price = orderOfAUserEntity.Price,
                    Dishes = currentDishBasketDtoList,
                    Address = orderOfAUserEntity.AddresGuid
                };
                return result;
            }
            catch (Exception e)
            {
                throw new BadHttpRequestException(e.Message);
            }
        }

        private async Task ValidateOrder(string token, Guid orderId)
        {
            var orderEntity = await _context.Orders.Where(x => x.Id == orderId).FirstOrDefaultAsync();
            if (orderEntity == null)
            {
                throw new NotFound("There is no order with such orderId");
            }
            var tokenInDB = await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync();
            var orderOfAUserEntity = await _context.Orders.Where(x => x.Id == orderId && x.User.Email == tokenInDB.userEmail).FirstOrDefaultAsync();
            if (orderOfAUserEntity == null)
            {
                throw new Forbidden("User is trying to find information about somebody else's order");
            }
        }
        private async Task ValidateToken(string token)
        {
            var tokenInDB = await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync();
            if (tokenInDB == null)
            {
                throw new Unauthorized("The token does not exist in database");
            }
            else if (tokenInDB.ExpirationDate < DateTime.Now.ToUniversalTime())
            {
                _context.Tokens.Remove(tokenInDB);
                await _context.SaveChangesAsync();
                throw new Forbidden("Token is expired");
            }
        }
    }
}
