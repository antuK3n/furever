using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using FurEver.Web.Data;

var builder = WebApplication.CreateBuilder(args);

var phCulture = new CultureInfo("en-PH");
CultureInfo.DefaultThreadCurrentCulture = phCulture;
CultureInfo.DefaultThreadCurrentUICulture = phCulture;

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

        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/Admin"))
            {
                var returnUrl = context.Request.Path + context.Request.QueryString;
                context.Response.Redirect("/Admin/Login" + QueryString.Create("ReturnUrl", returnUrl));
            }
            else
            {
                context.Response.Redirect(context.RedirectUri);
            }
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/Admin"))
                context.Response.Redirect("/Admin/Login");
            else
                context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
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

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        context.Response.Headers.Pragma = "no-cache";
        context.Response.Headers.Expires = "0";
    }
    await next();
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

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
