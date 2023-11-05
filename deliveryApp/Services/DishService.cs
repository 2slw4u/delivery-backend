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

        public async Task<DishPagedListDto> GetMenu(DishCategory[] categories, DishSorting sorting, int page = 1, bool vegetarian = false)
        {
            ValidateCategories(categories);
            ValidateSorting(sorting);
            try
            {
                //здесь я не знаю где получить стандартное кол-во блюд на странице
                double dishesPerPage = 6;
                var allDishes = await _context.Dishes.Where(x => (categories.Count() == 0 || categories.Contains(x.Category)) &&
                    (vegetarian == false || x.IsVegetarian == vegetarian)).ToListAsync();
                var amountOfPages = (int)Math.Ceiling(allDishes.Count() / dishesPerPage);
                ValidatePage(page, amountOfPages);
                allDishes = await GetSortedDishes(allDishes, sorting);
                var dishesOnSelectedPage = allDishes.Skip((int)dishesPerPage * (page - 1)).Take((int)Math.Min(dishesPerPage, _context.Dishes.Count() - (int)dishesPerPage * (page - 1))).ToList();
                var selectedDishes = new List<DishDto>();
                foreach (var dish in dishesOnSelectedPage)
                {
                    selectedDishes.Add(await GetDishInfo(dish.Id));
                }
                var result = new DishPagedListDto()
                {
                    Dishes = selectedDishes,
                    Pagination = new PageInfoModel()
                    {
                        Size = (int)dishesPerPage,
                        Count = amountOfPages,
                        Current = page
                    }
                };
                return result;
            }
            catch (Exception e)
            {
                throw new BadHttpRequestException(e.Message);
            }
        }

        public async Task SetRating(string token, Guid dishId, int ratingScore)
        {
            await ValidateToken(token);
            await ValidateDish(dishId);
            ValidateRating(ratingScore);
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
        private async Task<List<DishEntity>> GetSortedDishes(List<DishEntity>? dishes, DishSorting sortingType)
        {
            var result = new List<DishEntity>();
            switch (sortingType)
            {
                case DishSorting.NameAsc:
                    result = dishes.OrderBy(x => x.Name).ToList();
                    break;
                case DishSorting.NameDesc:
                    result = dishes.OrderByDescending(x => x.Name).ToList();
                    break;
                case DishSorting.PriceAsc:
                    result = dishes.OrderBy(x => x.Price).ToList();
                    break;
                case DishSorting.PriceDesc:
                    result = dishes.OrderByDescending(x => x.Price).ToList();
                    break;
                case DishSorting.RatingAsc:
                    result = dishes.OrderBy(x => GetDishRating(x.Id)).ToList();
                    break;
                case DishSorting.RatingDesc:
                    result = dishes.OrderByDescending(x => GetDishRating(x.Id)).ToList();
                    break;
                default:
                    throw new BadRequest("There is no such sorting type");
            }
            return result;
        }
        private static void ValidateCategories(DishCategory[] categories)
        {
            foreach (var category in categories)
            {
                if (!Enum.IsDefined(typeof(DishCategory), category))
                {
                    throw new BadRequest("There is no such dish category type");
                }
            }
        }
        private static void ValidatePage(int page, int amountOfPages)
        {
            if (page <= 0)
            {
                throw new BadRequest("Page must be a positive number");
            }
            if (page > amountOfPages)
            {
                throw new Conflict("Number of page can not be higher than overall amount of pages");
            }
        }
        private static void ValidateSorting(DishSorting sorting)
        {
            if (!Enum.IsDefined(typeof(DishSorting), sorting)) {
                throw new BadRequest("There is no such sorting type");
            }
        }
        private static void ValidateRating(int rating)
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
