using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantReviewApp.Data;



var builder = WebApplication.CreateBuilder(args);
var connectionString = "Data Source=restaurantdb.db";

// 💾 Σύνδεση με SQLite βάση δεδομένων
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// 🔐 Προσθήκη Identity (Login/Register)
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = false).AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// 📦 Προσθήκη υποστήριξης για Razor Pages και MVC Controllers
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// ⚙️ Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

// 🔐 Authentication & Authorization (ΣΩΣΤΗ σειρά)
app.UseAuthentication();
app.UseAuthorization();

// 📍 Χαρτογράφηση των routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 🧾 Razor Pages (για Identity UI: Login, Register κ.λπ.)
app.MapRazorPages();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    RestaurantReviewApp.Data.DbSeeder.SeedRestaurants(context);
    var services = scope.ServiceProvider;
    await RoleSeeder.SeedAdminsAsync(services);
}

app.Run();
