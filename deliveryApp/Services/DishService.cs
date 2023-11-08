using deliveryApp.Migrations;
using deliveryApp.Models;
using deliveryApp.Models.DTOs;
using deliveryApp.Models.Entities;
using deliveryApp.Models.Enums;
using deliveryApp.Models.Exceptions;
using deliveryApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace deliveryApp.Services
{
    public class DishService : IDishService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserService> _logger;
        int DISHES_PER_PAGE = 2;

        public DishService(AppDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddDishToMenu(DishDto dishModel, DishCategory dishCategory)
        {
            await ValidateDishDto(dishModel);
            await ValidateCategories(new DishCategory[1]{dishCategory});
            var result = new DishEntity()
            {
                Id = dishModel.Id,
                Name = dishModel.Name,
                Price = dishModel.Price,
                Description = dishModel.Description,
                IsVegetarian = dishModel.Vegetarian,
                Photo = dishModel.Image,
                Category = dishCategory
            };
            await _context.Dishes.AddAsync(result);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]Dish with {dishModel.Id} dishId has been added to the menu");
        }

        public async Task<bool> CheckIfUserCanSetRating(string token, Guid dishId)
        {
            await ValidateToken(token);
            await ValidateDish(dishId);
            var tokenEntity = await _context.Tokens.Where(x => x.Token == token).FirstOrDefaultAsync();
            var userEntity = await _context.Users.Where(x => x.Email == tokenEntity.userEmail).FirstOrDefaultAsync();
            var orders = await _context.Orders.Where(x => x.User == userEntity && x.Status == OrderStatus.Delievered).ToListAsync();
            foreach (var order in orders)
            {
                if (await _context.DishesInCart.Where(x => x.Order == order && x.Dish.Id == dishId).FirstOrDefaultAsync() != null)
                {
                    _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]It has been defined that user with token {token} is able to set a rating for dish with {dishId} Guid");
                    return true;
                }
            }
            _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]It has been defined that user with token {token} is unable to set a rating for dish with {dishId} Guid");
            return false;
        }

        public async Task<DishDto> GetDishInfo(Guid dishId)
        {
            await ValidateDish(dishId);
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
            _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]Information about dish with {dishId} Guid has been given out");
            return result;
        }

        public async Task<DishPagedListDto> GetMenu(DishCategory[] categories, DishSorting sorting, int page = 1, bool vegetarian = false)
        {
            await ValidateCategories(categories);
            await ValidateSorting(sorting);
            var allDishes = await _context.Dishes.Where(x => (categories.Count() == 0 || categories.Contains(x.Category)) &&
                (vegetarian == false || x.IsVegetarian == vegetarian)).ToListAsync();
            var amountOfPages = (int)Math.Ceiling(allDishes.Count() / (double)DISHES_PER_PAGE);
            await ValidatePage(page, amountOfPages);
            allDishes = await GetSortedDishes(allDishes, sorting);
            var dishesOnSelectedPage = allDishes.Skip(DISHES_PER_PAGE * (page - 1)).Take((int)Math.Min(DISHES_PER_PAGE , _context.Dishes.Count() - DISHES_PER_PAGE * (page - 1))).ToList();
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
                    Size = DISHES_PER_PAGE,
                    Count = amountOfPages,
                    Current = page
                }
            };
            _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]A menu has been given out");
            return result;
        }

        public async Task SetRating(string token, Guid dishId, int ratingScore)
        {
            await ValidateToken(token);
            await ValidateDish(dishId);
            await ValidateRating(ratingScore);
            if (await CheckIfUserCanSetRating(token, dishId) == false)
            {
                _logger.LogError($"[ERROR][DateTimeUTC: {DateTime.UtcNow}]User with token {token} tried to set the rating of a dish with {dishId} Guid and failed, because they have never ordered it");
                throw new Forbidden("User can only set the rating if he has ordered the dish before");
            }
            else
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
                var previousRating = await _context.Ratings.Where(x => x.User == userEntity && x.Dish == dishEntity).FirstOrDefaultAsync();
                if (previousRating != null)
                {
                    _context.Ratings.Remove(previousRating);
                }
                _context.Ratings.Add(result);
                await _context.SaveChangesAsync();
            }
            _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]User with token {token} has successfully set rating of a dish with {dishId} Guid");
        }
        private async Task<double?> GetDishRating(Guid dishId)
        {
            var dishesRatings = await _context.Ratings.Where(x => x.Dish.Id == dishId).Select(x => x.Value).ToListAsync();
            if (dishesRatings.Count == 0)
            {
                //или здесь нужно сказать, что блюда без рейтинга мы не рассматриваем?
                return null;
            }
            else
            {
                double result = 0;
                foreach (var rating in dishesRatings)
                {
                    result += rating;
                }
                return result/(double)dishesRatings.Count;
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
                    result = dishes.OrderBy(x => GetDishRating(x.Id).Result).Where(x => (GetDishRating(x.Id).Result != null)).ToList();
                    result.AddRange(dishes.Where(x => GetDishRating(x.Id).Result == null).ToList());
                    break;
                case DishSorting.RatingDesc:
                    result = dishes.OrderByDescending(x => GetDishRating(x.Id).Result).ToList();
                    break;
                default:
                    _logger.LogError($"[ERROR][DateTimeUTC: {DateTime.UtcNow}]SortingType {sortingType} should have been between 0 and 5");
                    throw new BadRequest("There is no given sorting type");
            }
            return result;
        }
        private async Task ValidateDishDto(DishDto dishModel)
        {
            if (dishModel.Price <= 0)
            {
                _logger.LogError($"[ERROR][DateTimeUTC: {DateTime.UtcNow}]Price {dishModel.Price} should have been a positive number");
                throw new BadRequest("Price should be a positive number");
            }
            _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]DishDto describing a Dish with {dishModel.Id} dishId has been validated");
        }
        private async Task ValidateCategories(DishCategory[] categories)
        {
            foreach (var category in categories)
            {
                if (!Enum.IsDefined(typeof(DishCategory), category))
                {
                    _logger.LogError($"[ERROR][DateTimeUTC: {DateTime.UtcNow}]Category {category} should have been between 0 and 4");
                    throw new BadRequest("There is no given category type");
                }
                _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]Category {category} has been validated");
            }
        }
        private async Task ValidatePage(int page, int amountOfPages)
        {
            if (page <= 0)
            {
                _logger.LogError($"[ERROR][DateTimeUTC: {DateTime.UtcNow}]Page({page}) should have been a positive number");
                throw new BadRequest("Page must be a positive number");
            }
            if (page > amountOfPages)
            {
                _logger.LogError($"[ERROR][DateTimeUTC: {DateTime.UtcNow}]Number of page({page}) should have been higher than overall amount of pages({amountOfPages})");
                throw new Conflict($"Number of page({page}) can not be higher than overall amount of pages({amountOfPages})");
            }
            _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]Page {page} has been validated");
        }
        private async Task ValidateSorting(DishSorting sorting)
        {
            if (!Enum.IsDefined(typeof(DishSorting), sorting))
            {
                _logger.LogError($"[ERROR][DateTimeUTC: {DateTime.UtcNow}]SortingType {sorting} should have been between 0 and 5");
                throw new BadRequest("There is no given sorting type");
            }
            _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]Sorting {sorting} has been validated");
        }
        private async Task ValidateRating(int rating)
        {
            if (rating < 0)
            {
                _logger.LogError($"[ERROR][DateTimeUTC: {DateTime.UtcNow}]Rating {rating} should have been a positive number");
                throw new BadRequest("Rating can not be a negative number");
            }
            if (rating > 10)
            {
                _logger.LogError($"[ERROR][DateTimeUTC: {DateTime.UtcNow}]Rating {rating} should have been between 0 and 10");
                throw new BadRequest($"Rating must be between 0 and 10, it can not be {rating}");
            }
            _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]Rating {rating} has been validated");
        }
        private async Task ValidateDish(Guid dishId)
        {
            var dishEntity = await _context.Dishes.Where(x => x.Id == dishId).FirstOrDefaultAsync();
            if (dishEntity == null)
            {
                _logger.LogError($"[ERROR][DateTimeUTC: {DateTime.UtcNow}]Dish with {dishId} has not been found in database");
                throw new NotFound($"There is no dish with {dishId} dishId");
            }
            _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]Dish with {dishId} dishId has been validated");
        }
        private async Task ValidateToken(string token)
        {
            var tokenInDB = await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync();
            if (tokenInDB == null)
            {
                _logger.LogError($"[ERROR][DateTimeUTC: {DateTime.UtcNow}]Token {token} has not been found in database");
                throw new Unauthorized($"The token does not exist in database (token: {token})");
            }
            else if (tokenInDB.ExpirationDate < DateTime.Now.ToUniversalTime())
            {
                _context.Tokens.Remove(tokenInDB);
                await _context.SaveChangesAsync();
                _logger.LogError($"[ERROR][DateTimeUTC: {DateTime.UtcNow}]Token {token} has expired");
                throw new Forbidden($"Token is expired (token: {token})");
            }
            _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]Token {token} has been validated");
        }
    }
}
