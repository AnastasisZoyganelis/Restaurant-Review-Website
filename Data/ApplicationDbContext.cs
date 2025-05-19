using Microsoft.EntityFrameworkCore;
using RestaurantReviewApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace RestaurantReviewApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Review> Reviews { get; set; }
        
        public DbSet<Category> Categories { get; set; }  // μόνο αν την έφτιαξες
    }
}
