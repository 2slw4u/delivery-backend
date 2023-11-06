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
        const int ORDER_AND_DELIVERY_DIFFERENCE = 60;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }
        public async Task ConfirmOrderDelivery(string token, Guid orderId)
        {
            await ValidateToken(token);
            await ValidateOrder(token, orderId);
            var tokenInDB = await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync();
            var orderOfAUserEntity = await _context.Orders.Where(x => x.Id == orderId && x.User.Email == tokenInDB.userEmail).FirstOrDefaultAsync();
            if (orderOfAUserEntity.Status == OrderStatus.Delievered)
            {
                throw new Conflict("Selected order already has confirmed delivery");
            }
            orderOfAUserEntity.Status = OrderStatus.Delievered;
            await _context.SaveChangesAsync();
        }

        public async Task CreateOrderFromCurrentBasket(string token, OrderCreateDto newOrder)
        {
            await ValidateToken(token);
            var tokenInDB = await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync();
            var dishesInOrder = await _context.DishesInCart.Where(x => x.User.Email == tokenInDB.userEmail).ToListAsync();
            if (dishesInOrder.Count == 0)
            {
                throw new BadRequest("There is no dishes in users current basket");
            }
            if ((newOrder.DeliveryTime - DateTime.Now).TotalMinutes < ORDER_AND_DELIVERY_DIFFERENCE)
            {
                throw new Forbidden("Delivery time must be at least an hour after order time");
            }
            var totalPrice = 0.0;
            foreach (var dish in dishesInOrder)
            {
                totalPrice += (dish.Price * dish.Amount);
            }
            var result = new OrderEntity()
            {
                Id = Guid.NewGuid(),
                DeliveryTime = newOrder.DeliveryTime,
                OrderTime = DateTime.Now,
                Price = totalPrice,
                AddresId = newOrder.AddressId.ToString(),
                Status = OrderStatus.InProcess,
                User = await _context.Users.Where(x => x.Email == tokenInDB.userEmail).FirstOrDefaultAsync()
            };
            await _context.Orders.AddAsync(result);
            await _context.SaveChangesAsync();
        }

        public async Task<List<OrderDto>> GetAllOrders(string token)
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

        public async Task<OrderDto> GetOrderInfo(string token, Guid orderId)
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
                DeliveryTime = orderOfAUserEntity.DeliveryTime,
                OrderTime = orderOfAUserEntity.OrderTime,
                Status = orderOfAUserEntity.Status,
                Price = orderOfAUserEntity.Price,
                Dishes = currentDishBasketDtoList,
                Address = orderOfAUserEntity.AddresId
            };
            return result;
        }

        private async Task ValidateOrder(string token, Guid orderId)
        {
            var orderEntity = await _context.Orders.Where(x => x.Id == orderId).FirstOrDefaultAsync();
            if (orderEntity == null)
            {
                throw new NotFound("There is no dish with such dishId");
            }
            var tokenInDB = await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync();
            var orderOfAUserEntity = await _context.Orders.Where(x => x.Id == orderId && x.User.Email == tokenInDB.userEmail).FirstOrDefaultAsync();
            if (orderOfAUserEntity  == null)
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
            else if (tokenInDB.ExpirationDate < DateTime.Now)
            {
                _context.Tokens.Remove(tokenInDB);
                await _context.SaveChangesAsync();
                throw new Forbidden("Token is expired");
            }
        }
    }
}
