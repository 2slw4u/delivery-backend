﻿using deliveryApp.Models;
using deliveryApp.Models.DTOs;
using deliveryApp.Models.Entities;
using deliveryApp.Models.Exceptions;
using deliveryApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace deliveryApp.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }
        public Task ConfirmOrderDelivery(string token, Guid orderId)
        {
            throw new NotImplementedException();
        }

        public Task<OrderCreateDto> CreateOrderFromCurrentBasket(string token, DateTime deliveryTime, Guid addresId)
        {
            throw new NotImplementedException();
        }

        public Task<List<OrderDto>> GetAllOrders(string token)
        {
            throw new NotImplementedException();
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
                Address = orderOfAUserEntity.Address
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
