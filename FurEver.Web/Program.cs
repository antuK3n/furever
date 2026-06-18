using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using FurEver.Web.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<FurEverContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FurEver")));

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Startup database tasks:
// 1. Seed the default admin account (BCrypt hash must be generated in C#).
// 2. Mark overdue vaccinations (replaces the MySQL daily event;
//    SQL Server Express has no Agent).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FurEverContext>();
    try
    {
        if (!await db.Admins.AnyAsync())
        {
            db.Admins.Add(new FurEver.Web.Models.Admin
            {
                Email = "admin@furever.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123", workFactor: 10),
                FullName = "FurEver Administrator"
            });
            await db.SaveChangesAsync();
            app.Logger.LogInformation("Seeded default admin account (admin@furever.com).");
        }

        await db.Database.ExecuteSqlRawAsync("EXEC dbo.sp_update_overdue_vaccinations");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Startup database tasks failed (database may not be ready yet).");
    }
}

app.Run();
