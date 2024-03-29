﻿using deliveryApp.Models;
using deliveryApp.Models.DTOs;
using deliveryApp.Models.Entities;
using deliveryApp.Models.Enums;
using deliveryApp.Models.Exceptions;
using deliveryApp.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace deliveryApp.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IAddressService _addressService;
        private readonly ILogger<UserService> _logger;
        const int ORDER_AND_DELIVERY_DIFFERENCE = 60;

        public OrderService(AppDbContext context, IAddressService addressService, ILogger<UserService> logger)
        {
            _context = context;
            _addressService = addressService;
            _logger = logger;
        }
        public async Task ConfirmOrderDelivery(HttpContext httpContext,  Guid orderId)
        {
            var token = httpContext.Request.Headers["Authorization"].First().Replace("Bearer ", "");
            await ValidateOrder(token, orderId);
            var tokenInDB = await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync();
            var orderOfAUserEntity = await _context.Orders.Where(x => x.Id == orderId && x.User.Email == tokenInDB.userEmail).FirstOrDefaultAsync();
            if (orderOfAUserEntity.Status == OrderStatus.Delievered)
            {
                _logger.LogError($"[ERROR][DateTimeUTC: {DateTime.UtcNow}]User with token {token} has tried to confirm delivery of {orderId} order, which has already been confirmed before");
                throw new Conflict($"Order {orderId} has already confirmed delivery");
            }
            orderOfAUserEntity.Status = OrderStatus.Delievered;
            await _context.SaveChangesAsync();
            _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]User with token {token} has confirmed delivery of order with {orderId} orderId");
        }

        public async Task CreateOrderFromCurrentBasket(HttpContext httpContext, OrderCreateDto newOrder)
        {
            var token = httpContext.Request.Headers["Authorization"].First().Replace("Bearer ", "");
            var tokenInDB = await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync();
            var dishesInOrder = await _context.DishesInCart.Where(x => x.User.Email == tokenInDB.userEmail && x.Order == null).ToListAsync();
            if (dishesInOrder.Count == 0)
            {
                _logger.LogError($"[ERROR][DateTimeUTC: {DateTime.UtcNow}]User with token {token} has no dishes in current basket");
                throw new BadRequest("There is no dishes in users current basket");
            }
            if ((newOrder.DeliveryTime - DateTime.Now.ToUniversalTime()).TotalMinutes < ORDER_AND_DELIVERY_DIFFERENCE)
            {
                _logger.LogError($"[ERROR][DateTimeUTC: {DateTime.UtcNow}]DeliveryTime was {newOrder.DeliveryTime} and difference between it and current time was less than {ORDER_AND_DELIVERY_DIFFERENCE} (less than it should have)");
                throw new Forbidden("Delivery time must be at least an hour after order time");
            }
            var totalPrice = 0.0;
            foreach (var dish in dishesInOrder)
            {
                totalPrice += (dish.Price * dish.Amount);
            }
            await _addressService.ValidateAddressGuid(newOrder.AddressId);
            var textAddress = await _addressService.GetChain(newOrder.AddressId);
            if (textAddress.LastOrDefault().ObjectLevelText != "Здание (сооружение)")
            {
                _logger.LogError($"[ERROR][DateTimeUTC: {DateTime.UtcNow}]User with token {token} has tried to make an order with address Guid {newOrder.AddressId}. " +
                    $"\nIt was unsuccessful beacuse address objectlevel was {textAddress.LastOrDefault().ObjectLevelText} and not Building");
                throw new BadRequest($"Order's address Guid can not be {newOrder.AddressId}, because this Guid does not refer to a building");
            }
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
            _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]User with token {token} has created order with {result.Id} from dishes in their basket");
        }

        public async Task<List<OrderDto>> GetAllOrders(HttpContext httpContext)
        {
            var token = httpContext.Request.Headers["Authorization"].First().Replace("Bearer ", "");
            var tokenInDB = await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync();
            var allOrders = await _context.Orders.Where(x => x.User.Email == tokenInDB.userEmail).Select(x => x.Id).ToListAsync();
            var result = new List<OrderDto>();
            foreach (var order in allOrders)
            {
                result.Add(await GetOrderInfo(httpContext, order));
            }
            _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]User with token {token} has received information about all of their orders");
            return result;
        }

        public async Task<OrderDto> GetOrderInfo(HttpContext httpContext, Guid orderId)
        {
            var token = httpContext.Request.Headers["Authorization"].First().Replace("Bearer ", "");
            await ValidateOrder(token, orderId);
            var tokenInDB = await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync();
            var orderOfAUserEntity = await _context.Orders.Where(x => x.Id == orderId && x.User.Email == tokenInDB.userEmail).FirstOrDefaultAsync();
            var currentDishBasketDtoList = new List<DishBasketDto>();
            var dishesInOrder = await _context.DishesInCart.Where(x => x.Order == orderOfAUserEntity).Select(x => new {x.Id, x.Dish, x.Price, x.Amount}).ToListAsync();
            foreach (var DishInCart in dishesInOrder)
            {
                currentDishBasketDtoList.Add(new DishBasketDto()
                {
                    Id = DishInCart.Dish.Id,
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
            _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]User with token {token} has received information about order with {orderId} orderId");
            return result;
        }

        private async Task ValidateOrder(string token, Guid orderId)
        {
            var orderEntity = await _context.Orders.Where(x => x.Id == orderId).FirstOrDefaultAsync();
            if (orderEntity == null)
            {
                _logger.LogError($"[ERROR][DateTimeUTC: {DateTime.UtcNow}]Order with {orderId} has not been found in database");
                throw new NotFound($"There is no order with {orderId} orderId");
            }
            var tokenInDB = await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync();
            var orderOfAUserEntity = await _context.Orders.Where(x => x.Id == orderId && x.User.Email == tokenInDB.userEmail).FirstOrDefaultAsync();
            if (orderOfAUserEntity == null)
            {
                _logger.LogError($"[ERROR][DateTimeUTC: {DateTime.UtcNow}]User with token {token} is trying to get information about somebody else's order");
                throw new Forbidden("User is trying to find information about somebody else's order");
            }
            _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]Order with {orderId} has been validated");
        }
    }
}
