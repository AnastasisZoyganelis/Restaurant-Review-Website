using Microsoft.AspNetCore.Identity;

namespace RestaurantReviewApp.Data
{
    public static class RoleSeeder
    {
        public static async Task SeedAdminsAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Δημιουργία ρόλου Admin
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            var adminEmails = new[]
            {
                "admin@example.com",
                "zoukisouki@gmail.com",
                "testadmin@site.gr"
            };

            foreach (var email in adminEmails)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new IdentityUser { UserName = email, Email = email };
                    await userManager.CreateAsync(user, "Admin123!"); // Default password
                }

                if (!await userManager.IsInRoleAsync(user, "Admin"))
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}
