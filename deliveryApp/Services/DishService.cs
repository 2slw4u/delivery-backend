using deliveryApp.Models;
using deliveryApp.Models.DTOs;
using deliveryApp.Models.Entities;
using deliveryApp.Models.Enums;
using deliveryApp.Models.Exceptions;
using deliveryApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace deliveryApp.Services
{
    public class DishService : IDishService
    {
        private readonly AppDbContext _context;

        public DishService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<bool> CheckIfUserCanSetRating(string token, Guid dishId)
        {
            await ValidateToken(token);
            await ValidateDish(dishId);
            try
            {
                var tokenEntity = await _context.Tokens.Where(x => x.Token == token).FirstOrDefaultAsync();
                var userEntity = await _context.Users.Where(x => x.Email == tokenEntity.userEmail).FirstOrDefaultAsync();
                var orders = await _context.Orders.Where(x => x.User == userEntity && x.Status == OrderStatus.Delievered).ToListAsync();
                foreach (var order in orders)
                {
                    if (await _context.DishesInCart.Where(x => x.Order == order && x.Dish.Id == dishId).FirstOrDefaultAsync() != null)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                throw new BadHttpRequestException(e.Message);
            }
        }

        public async Task<DishDto> GetDishInfo(Guid dishId)
        {
            await ValidateDish(dishId);
            try
            {
                var dishEntity = await _context.Dishes.Where(x => x.Id == dishId).FirstOrDefaultAsync();
                var result = new DishDto()
                {
                    Id = dishId,
                    Name = dishEntity.Name,
                    Description = dishEntity.Description,
                    Price = dishEntity.Price,
                    Image = dishEntity.Photo,
                    Vegetarian = dishEntity.IsVegetarian,
                    Rating = await GetDishRating(dishId),
                    Category = dishEntity.Category

                };
                return result;
            }
            catch (Exception e)
            {
                throw new BadHttpRequestException(e.Message);
            }
        }

        public async Task<DishPagedListDto> GetMenu(DishCategory[] categories, string sorting, int page, bool vegetarian = false)
        {
            throw new NotImplementedException();
        }

        public async Task SetRating(string token, Guid dishId, int ratingScore)
        {
            await ValidateToken(token);
            await ValidateDish(dishId);
            await ValidateRating(ratingScore);
            if (await CheckIfUserCanSetRating(token, dishId) == false) 
            {
                throw new Forbidden("User can only set the rating if he has ordered the dish before");
            }
            else
            {
                try
                {
                    var dishEntity = await _context.Dishes.Where(x => x.Id == dishId).FirstOrDefaultAsync();
                    var tokenInDB = await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync();
                    var userEntity = await _context.Users.Where(x => x.Email == tokenInDB.userEmail).FirstOrDefaultAsync();
                    var result = new RatingEntity()
                    {
                        Id = Guid.NewGuid(),
                        Value = ratingScore,
                        Dish = dishEntity,
                        User = userEntity
                    };
                }
                catch (Exception e)
                {
                    throw new BadHttpRequestException(e.Message);
                }
            }
            throw new NotImplementedException();
        }
        private async Task<double?> GetDishRating(Guid dishId)
        {
            var dishesRatings = await _context.Ratings.Where(x => x.Id == dishId).ToListAsync();
            if (dishesRatings.Count == 0)
            {
                return null;
            }
            else
            {
                double result = 0;
                foreach (var rating in dishesRatings)
                {
                    result += rating.Value;
                }
                return result/dishesRatings.Count;
            }
            
        }
        private async Task ValidateRating(int rating)
        {
            if (rating < 0)
            {
                throw new BadRequest("Rating can not be a negative number");
            }
            if (rating > 10)
            {
                throw new BadRequest("Rating must be between 0 and 10");
            }
        }
        private async Task ValidateDish(Guid dishId)
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
