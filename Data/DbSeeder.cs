using RestaurantReviewApp.Models;

namespace RestaurantReviewApp.Data
{
    public static class DbSeeder
    {
        public static void SeedRestaurants(ApplicationDbContext context)
        {
            if (context.Restaurants.Any()) return;

            var rnd = new Random();
            var cities = new[] { "Αθήνα", "Θεσσαλονίκη", "Πάτρα", "Λάρισα", "Ηράκλειο", "Βόλος", "Καλαμάτα" };
            var types = new[] { "Taverna", "Grill", "Pizza", "Sushi", "Burger", "Bistro", "Café", "Bakery","Indian","Chinese","Ethnic" };

            for (int i = 1; i <= 150; i++)
            {
                var type = types[rnd.Next(types.Length)];
                var city = cities[rnd.Next(cities.Length)];
                var name = $"{type} {i}";
                var description = $"Δοκιμάστε το {type.ToLower()} μας στο {city}!";
                var average = 0;

                context.Restaurants.Add(new Restaurant
                {
                    Name = name,
                    Description = description,
                    Location = city,
                    Category = type,
                    AverageRating = average
                });
            }

            context.SaveChanges();
        }
    }
}
