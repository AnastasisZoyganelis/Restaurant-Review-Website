using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RestaurantReviewApp.Data;
using RestaurantReviewApp.Models;

namespace RestaurantReviewApp.Controllers
{
    public class RestaurantsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RestaurantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Restaurants
        public async Task<IActionResult> Index(string searchName, string cityFilter, string categoryFilter, string sortOrder)
        {
            // Ξεκινάμε με queryable
            var restaurantsQuery = _context.Restaurants
                .Include(r => r.Reviews)
                .AsQueryable();

            // Εφαρμογή φίλτρων
            if (!string.IsNullOrEmpty(searchName))
                restaurantsQuery = restaurantsQuery.Where(r => r.Name.Contains(searchName));

            if (!string.IsNullOrEmpty(cityFilter))
                restaurantsQuery = restaurantsQuery.Where(r => r.Location == cityFilter);

            if (!string.IsNullOrEmpty(categoryFilter))
                restaurantsQuery = restaurantsQuery.Where(r => r.Category == categoryFilter);

            // Εκτέλεση query
            var restaurants = await restaurantsQuery.ToListAsync();

            // Υπολογισμός global average για Bayesian
            var allReviews = restaurants.SelectMany(r => r.Reviews).ToList();
            double globalAverage = allReviews.Any() ? allReviews.Average(r => r.Rating) : 0;
            int m = 5;

            // Υπολογισμός βαθμολογιών
            foreach (var r in restaurants)
            {
                r.DisplayRating = r.Reviews.Any()
                    ? Math.Round(r.Reviews.Average(rv => rv.Rating), 1)
                    : 0;

                double R = r.DisplayRating;
                int v = r.Reviews.Count;

                r.AverageRating = v == 0
                    ? 0
                    : Math.Round(((v / (double)(v + m)) * R + (m / (double)(v + m)) * globalAverage), 2);
            }

            // Ταξινόμηση
            if (sortOrder == "rating_desc")
                restaurants = restaurants.OrderByDescending(r => r.AverageRating).ToList();

            // ViewBag για dropdown φίλτρα
            ViewBag.Cities = await _context.Restaurants.Select(r => r.Location).Distinct().ToListAsync();
            ViewBag.Categories = await _context.Restaurants.Select(r => r.Category).Distinct().ToListAsync();

            return View(restaurants);
        }

        // GET: Restaurants/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurants.Include(r => r.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (restaurant == null)
            {
                return NotFound();
            }

            return View(restaurant);
        }

        // GET: Restaurants/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Restaurants/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Location,AverageRating")] Restaurant restaurant)
        {
            if (ModelState.IsValid)
            {
                _context.Add(restaurant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(restaurant);
        }

        // GET: Restaurants/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null)
            {
                return NotFound();
            }
            return View(restaurant);
        }

        // POST: Restaurants/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Location,AverageRating")] Restaurant restaurant)
        {
            if (id != restaurant.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(restaurant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RestaurantExists(restaurant.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(restaurant);
        }

        // GET: Restaurants/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(m => m.Id == id);
            if (restaurant == null)
            {
                return NotFound();
            }

            return View(restaurant);
        }

        // POST: Restaurants/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var restaurant = await _context.Restaurants
            .Include(r => r.Reviews)
            .FirstOrDefaultAsync(m => m.Id == id);
            if (restaurant != null)
            {
                _context.Restaurants.Remove(restaurant);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RestaurantExists(int id)
        {
            return _context.Restaurants.Any(e => e.Id == id);
        }
    }
}
