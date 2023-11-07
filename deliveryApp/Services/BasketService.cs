using deliveryApp.Models;
using deliveryApp.Models.DTOs;
using deliveryApp.Models.Entities;
using deliveryApp.Models.Exceptions;
using deliveryApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace deliveryApp.Services
{
    public class BasketService : IBasketService
    {
        private readonly AppDbContext _context;

        public BasketService(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddDish(string token, Guid dishId)
        {
            try
            {
                await ValidateToken(token);
                await ValidateDish(dishId);
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
                    dishInCart.Amount++;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new BadHttpRequestException(e.Message);
            }
        }

        public async Task<List<DishBasketDto>> Get(string token)
        {
            try
            {
                await ValidateToken(token);
                var tokenEntity = await _context.Tokens.Where(x => x.Token == token).FirstOrDefaultAsync();
                var dishes = await _context.DishesInCart.Include(x => x.User).Include(x => x.Dish).Where(x => x.User.Email == tokenEntity.userEmail && x.Order == null).ToListAsync();
                var result = new List<DishBasketDto>();
                foreach (var dish in dishes)
                {
                    DishBasketDto currentDto = new DishBasketDto()
                    {
                        Id = dish.Dish.Id,
                        Name = dish.Dish.Name,
                        Price = dish.Dish.Price,
                        Amount = dish.Amount,
                        TotalPrice = dish.Amount * dish.Dish.Price,
                        Image = dish.Dish.Photo
                    };
                }
                return result;
            }
            catch (Exception e)
            {
                throw new BadHttpRequestException(e.Message);
            }
        }

        public async Task RemoveDish(string token, Guid dishId, bool increase = false)
        {
            try
            {
                await ValidateToken(token);
                await ValidateDish(dishId);
                var tokenEntity = await _context.Tokens.Where(x => x.Token == token).FirstOrDefaultAsync();
                var dishInCart = await _context.DishesInCart.Where(x => x.User.Email == tokenEntity.userEmail && x.Dish.Id == dishId && x.Order == null).FirstOrDefaultAsync();
                if (increase)
                {
                    dishInCart.Amount--;
                }
                else
                {
                    _context.DishesInCart.Remove(dishInCart);
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new BadHttpRequestException(e.Message);
            }
        }

        private async Task ValidateDish (Guid dishId)
        {
            var dishEntity = await _context.Dishes.Where(x => x.Id == dishId).FirstOrDefaultAsync();
            if (dishEntity == null)
            {
                throw new NotFound("There is no dish with such dishId");
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
