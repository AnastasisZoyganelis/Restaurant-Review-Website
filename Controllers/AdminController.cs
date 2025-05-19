using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantReviewApp.Data;

namespace RestaurantReviewApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var restaurantCount = await _context.Restaurants.CountAsync();
            var reviewCount = await _context.Reviews.CountAsync();
            var userCount = await _context.Users.CountAsync();

            var reviews = await _context.Reviews
                .Include(r => r.Restaurant)
                .OrderByDescending(r => r.DatePosted)
                .ToListAsync();

            ViewBag.RestaurantCount = restaurantCount;
            ViewBag.ReviewCount = reviewCount;
            ViewBag.UserCount = userCount;

            return View(reviews);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                // Ανανεώνει το AverageRating του εστιατορίου
                var restaurant = await _context.Restaurants
                    .Include(r => r.Reviews)
                    .FirstOrDefaultAsync(r => r.Id == review.RestaurantId);

                if (restaurant != null)
                {
                    int v = restaurant.Reviews.Count;
                    double R = v > 0 ? restaurant.Reviews.Average(r => r.Rating) : 0;
                    double globalAvg = 3.5;
                    int m = 5;

                    restaurant.AverageRating = v == 0
                        ? 0
                        : Math.Round(((v / (double)(v + m)) * R + (m / (double)(v + m)) * globalAvg), 2);

                    _context.Update(restaurant);
                    await _context.SaveChangesAsync();
                }

                TempData["Message"] = "Η αξιολόγηση διαγράφηκε.";
            }

            return RedirectToAction("Index");
        }

    }
}
