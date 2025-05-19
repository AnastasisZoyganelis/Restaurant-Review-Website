using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantReviewApp.Data;
using RestaurantReviewApp.Models;

namespace RestaurantReviewApp.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }
        private async Task UpdateRestaurantAverage(int restaurantId)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Reviews)
                .FirstOrDefaultAsync(r => r.Id == restaurantId);

            if (restaurant != null)
            {
                int v = restaurant.Reviews.Count;
                double R = v > 0 ? restaurant.Reviews.Average(r => r.Rating) : 0;
                double globalAverage = 3.5;
                int m = 5;

                restaurant.AverageRating = v == 0
                    ? 0
                    : Math.Round(((v / (double)(v + m)) * R + (m / (double)(v + m)) * globalAverage), 2);

                _context.Update(restaurant);
                await _context.SaveChangesAsync();
            }
        }

        // GET: Reviews
        public async Task<IActionResult> Index()
        {
            var reviews = _context.Reviews.Include(r => r.Restaurant);
            return View(await reviews.ToListAsync());
        }

        // GET: Reviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.Reviews
                .Include(r => r.Restaurant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (review == null) return NotFound();

            return View(review);
        }

        // GET: Reviews/Create
        [Authorize]
        public IActionResult Create(int restaurantId)
        {
            var review = new Review
            {
                RestaurantId = restaurantId
            };
            return View(review);
        }

        // POST: Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("RestaurantId,Rating,Comment")] Review review)
        {
            if (ModelState.IsValid)
            {
                review.UserName = User.Identity?.Name ?? "Ανώνυμος";
                review.DatePosted = DateTime.Now;

                _context.Add(review);
                await _context.SaveChangesAsync();

                await UpdateRestaurantAverage(review.RestaurantId);

                return RedirectToAction("Details", "Restaurants", new { id = review.RestaurantId });
            }

            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", review.RestaurantId);
            return View(review);
        }

        // GET: Reviews/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            if (review.UserName != User.Identity?.Name)
                return Forbid();

            return View(review);
        }

        // POST: Reviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RestaurantId,Rating,Comment")] Review review)
        {
            var existingReview = await _context.Reviews.FindAsync(id);
            if (existingReview == null || existingReview.UserName != User.Identity?.Name)
                return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    existingReview.Rating = review.Rating;
                    existingReview.Comment = review.Comment;
                    _context.Update(existingReview);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction("Details", "Restaurants", new { id = review.RestaurantId });
            }

            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", review.RestaurantId);
            return View(review);
        }

        // GET: Reviews/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.Reviews
                .Include(r => r.Restaurant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (review == null) return NotFound();

            if (review.UserName != User.Identity?.Name)
                return Forbid();

            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null && review.UserName == User.Identity?.Name)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Restaurants", new { id = review?.RestaurantId });
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.Id == id);
        }
    }
}
