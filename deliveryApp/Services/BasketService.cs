using deliveryApp.Models;
using deliveryApp.Models.DTOs;
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
        public Task AddDish(string token, Guid dishId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<DishBasketDto>> Get(string token)
        {
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

        public Task RemoveDish(string token, Guid dishId, bool increase = false)
        {
            throw new NotImplementedException();
        }
    }
}
