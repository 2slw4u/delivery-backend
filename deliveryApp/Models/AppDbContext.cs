using deliveryApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace deliveryApp.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<DishEntity> Dishes { get; set; }
        public DbSet<DishInCartEntity> DishesInCart { get; set; }
        public DbSet<OrderEntity> Orders { get; set; }
        public DbSet<RatingEntity> Ratings { get; set; }
        public DbSet<TokenEntity> Tokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserEntity>().HasIndex(x => x.FullName);
            base.OnModelCreating(modelBuilder);
        }
    }
}
