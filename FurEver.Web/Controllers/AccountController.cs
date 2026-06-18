using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FurEver.Web.Data;
using FurEver.Web.Models;
using FurEver.Web.Models.ViewModels;

namespace FurEver.Web.Controllers;

public class AccountController : Controller
{
    private readonly FurEverContext _db;

    public AccountController(FurEverContext db) => _db = db;

    // ---------- Register ----------

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new RegisterViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var emailTaken = await _db.Adopters.AnyAsync(a => a.Email == model.Email);
        if (emailTaken)
        {
            ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
            return View(model);
        }

        var adopter = new Adopter
        {
            Email = model.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password, workFactor: 10),
            FullName = model.FullName,
            ContactNo = model.ContactNo,
            Address = model.Address,
            HousingType = model.HousingType,
            HasOtherPets = model.HasOtherPets,
            HasChildren = model.HasChildren,
            ExperienceLevel = model.ExperienceLevel
        };

        _db.Adopters.Add(adopter);
        await _db.SaveChangesAsync();

        await SignInAdopterAsync(adopter);
        TempData["Success"] = $"Welcome to FurEver, {adopter.FullName}!";
        return RedirectToAction("Index", "Home");
    }

    // ---------- Login ----------

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // Member login authenticates ONLY against the Adopter table.
        // Admins sign in through the separate /Admin/Login page.
        var adopter = await _db.Adopters.FirstOrDefaultAsync(a => a.Email == model.Email);
        if (adopter is not null && BCrypt.Net.BCrypt.Verify(model.Password, adopter.PasswordHash))
        {
            await SignInAdopterAsync(adopter);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(model);
    }

    // ---------- Logout ----------

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    // ---------- Profile ----------

    [Authorize(Roles = "Adopter")]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var adopter = await CurrentAdopterAsync();
        if (adopter is null) return RedirectToAction(nameof(Login));

        var model = new ProfileViewModel
        {
            AdopterId = adopter.AdopterId,
            Email = adopter.Email,
            FullName = adopter.FullName,
            ContactNo = adopter.ContactNo,
            Address = adopter.Address,
            HousingType = adopter.HousingType,
            HasOtherPets = adopter.HasOtherPets,
            HasChildren = adopter.HasChildren,
            ExperienceLevel = adopter.ExperienceLevel
        };

        await PopulateStatsAsync(model, adopter.AdopterId);
        return View(model);
    }

    [Authorize(Roles = "Adopter")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var adopter = await CurrentAdopterAsync();
        if (adopter is null) return RedirectToAction(nameof(Login));

        // Password change is optional; validate only when requested.
        var wantsPasswordChange =
            !string.IsNullOrEmpty(model.CurrentPassword) ||
            !string.IsNullOrEmpty(model.NewPassword) ||
            !string.IsNullOrEmpty(model.ConfirmNewPassword);

        if (wantsPasswordChange)
        {
            if (string.IsNullOrEmpty(model.CurrentPassword) ||
                !BCrypt.Net.BCrypt.Verify(model.CurrentPassword, adopter.PasswordHash))
                ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is incorrect.");

            if (string.IsNullOrEmpty(model.NewPassword))
                ModelState.AddModelError(nameof(model.NewPassword), "Enter a new password.");
        }

        if (!ModelState.IsValid)
        {
            model.Email = adopter.Email;
            await PopulateStatsAsync(model, adopter.AdopterId);
            return View(model);
        }

        adopter.FullName = model.FullName;
        adopter.ContactNo = model.ContactNo;
        adopter.Address = model.Address;
        adopter.HousingType = model.HousingType;
        adopter.HasOtherPets = model.HasOtherPets;
        adopter.HasChildren = model.HasChildren;
        adopter.ExperienceLevel = model.ExperienceLevel;

        if (wantsPasswordChange)
            adopter.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword, workFactor: 10);

        await _db.SaveChangesAsync();

        TempData["Success"] = wantsPasswordChange ? "Profile and password updated." : "Profile updated.";
        return RedirectToAction(nameof(Profile));
    }

    private async Task PopulateStatsAsync(ProfileViewModel model, int adopterId)
    {
        var adoptions = await _db.Adoptions
            .Where(a => a.AdopterId == adopterId)
            .Include(a => a.Pet)
            .OrderByDescending(a => a.ApplicationDate)
            .ThenByDescending(a => a.AdoptionId)
            .ToListAsync();

        model.TotalApplications = adoptions.Count;
        model.CompletedAdoptions = adoptions.Count(a => a.Status == "Completed");
        model.PendingApplications = adoptions.Count(a => a.Status == "Pending");
        model.RecentApplications = adoptions.Take(3).ToList();

        model.FavoritesCount = await _db.Favorites.CountAsync(f => f.AdopterId == adopterId);
        model.RecentFavorites = await _db.Favorites
            .Where(f => f.AdopterId == adopterId)
            .Include(f => f.Pet)
            .OrderByDescending(f => f.DateAdded)
            .Take(4)
            .ToListAsync();
    }

    // ---------- Helpers ----------

    private async Task SignInAdopterAsync(Adopter adopter)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, adopter.AdopterId.ToString()),
            new(ClaimTypes.Name, adopter.FullName),
            new(ClaimTypes.Email, adopter.Email),
            new(ClaimTypes.Role, "Adopter")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });
    }

    private async Task<Adopter?> CurrentAdopterAsync()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var id)) return null;
        return await _db.Adopters.FindAsync(id);
    }
}
