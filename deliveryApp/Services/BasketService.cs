using deliveryApp.Models;
using deliveryApp.Models.DTOs;
using deliveryApp.Models.Entities;
using deliveryApp.Models.Exceptions;
using deliveryApp.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace deliveryApp.Services
{
    public class BasketService : IBasketService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserService> _logger;
        private readonly IDishService _dishService;

        public BasketService(AppDbContext context, ILogger<UserService> logger, IDishService dishService)
        {
            _context = context;
            _logger = logger;
            _dishService = dishService;
        }
        public async Task AddDish(HttpContext httpContext, Guid dishId)
        {
            var token = httpContext.Request.Headers["Authorization"].First().Replace("Bearer ", "");
            await _dishService.ValidateDish(dishId);
            var tokenEntity = await _context.Tokens.Where(x => x.Token == token).FirstOrDefaultAsync();
            var userEntity = await _context.Users.Where(x => x.Email == tokenEntity.userEmail).FirstOrDefaultAsync();
            var dishEntity = await _context.Dishes.Where(x => x.Id == dishId).FirstOrDefaultAsync();
            var dishInCart = await _context.DishesInCart.Where(x => x.User.Email == tokenEntity.userEmail && x.Dish.Id == dishId && x.Order == null).FirstOrDefaultAsync();
            if (dishInCart == null)
            {
                var newDishInCart = new DishInCartEntity()
                {
                    Id = Guid.NewGuid(),
                    Amount = 1,
                    User = userEntity,
                    Price = dishEntity.Price,
                    Dish = dishEntity,
                    Order = null
                };
                await _context.DishesInCart.AddAsync(newDishInCart);
            }
            else
            {
                dishInCart.Amount += 1;
            }
            _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]Dish with {dishId} Guid has been added to current baset of a user with token {token}");
            await _context.SaveChangesAsync();
        }

        public async Task<List<DishBasketDto>> Get(HttpContext httpContext)
        {
            var token = httpContext.Request.Headers["Authorization"].First().Replace("Bearer ", "");
            var tokenEntity = await _context.Tokens.Where(x => x.Token == token).FirstOrDefaultAsync();
            var userEntity = await _context.Users.Where(x => x.Email == tokenEntity.userEmail).FirstOrDefaultAsync();
            var dishesInCart = await _context.DishesInCart.Where(x => x.User == userEntity && x.Order == null && x.Dish != null).Select(x => new {x.Dish, x.Amount, x.Price}).ToListAsync();
            var dishInCart = dishesInCart.FirstOrDefault();
            var result = new List<DishBasketDto>();
            foreach (var dish in dishesInCart)
            {
                var currentDto = new DishBasketDto()
                {
                    Id = dish.Dish.Id,
                    Name = dish.Dish.Name,
                    Price = dish.Price,
                    TotalPrice = dish.Amount * dish.Price,
                    Amount = dish.Amount,
                    Image = dish.Dish.Photo
                };
                result.Add(currentDto);
            }
            _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]User with token {token} has received information about dishes in his current basket");
            return result;
        }

        public async Task RemoveDish(HttpContext httpContext, Guid dishId, bool increase = false)
        {
            var token = httpContext.Request.Headers["Authorization"].First().Replace("Bearer ", "");
            await _dishService.ValidateDish(dishId);
            var tokenEntity = await _context.Tokens.Where(x => x.Token == token).FirstOrDefaultAsync();
            var dishInCart = await _context.DishesInCart.Where(x => x.User.Email == tokenEntity.userEmail && x.Dish.Id == dishId && x.Order.Id == null).FirstOrDefaultAsync();
            if (dishInCart == null)
            {
                _logger.LogError($"[ERROR][DateTimeUTC: {DateTime.UtcNow}]User with token {token} unsuccessfully tried to remove a dish with a {dishId} Guid from the cart, although it weren't there from the beginning");
                throw new BadRequest($"There is no dish in cart with {dishId} Guid");
            }
            if (increase && dishInCart.Amount > 1)
            {
                dishInCart.Amount--;
            }
            else
            {
                _context.DishesInCart.Remove(dishInCart);
            }
            await _context.SaveChangesAsync();
            _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]Dish with {dishId} Guid has been removed from current baset of a user with token {token}");
        }
    }
}
