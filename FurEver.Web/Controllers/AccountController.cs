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

        var adopter = await _db.Adopters.FirstOrDefaultAsync(a => a.Email == model.Email);
        if (adopter is not null && BCrypt.Net.BCrypt.Verify(model.Password, adopter.PasswordHash))
        {
            await SignInAdopterAsync(adopter);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        // Not an adopter — try the Admin table.
        var admin = await _db.Admins.FirstOrDefaultAsync(a => a.Email == model.Email);
        if (admin is not null && BCrypt.Net.BCrypt.Verify(model.Password, admin.PasswordHash))
        {
            await SignInAdminAsync(admin);
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
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

        return View(new ProfileViewModel
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
        });
    }

    [Authorize(Roles = "Adopter")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var adopter = await CurrentAdopterAsync();
        if (adopter is null) return RedirectToAction(nameof(Login));

        if (!ModelState.IsValid)
        {
            model.Email = adopter.Email;
            return View(model);
        }

        adopter.FullName = model.FullName;
        adopter.ContactNo = model.ContactNo;
        adopter.Address = model.Address;
        adopter.HousingType = model.HousingType;
        adopter.HasOtherPets = model.HasOtherPets;
        adopter.HasChildren = model.HasChildren;
        adopter.ExperienceLevel = model.ExperienceLevel;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Profile updated.";
        return RedirectToAction(nameof(Profile));
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

    private async Task SignInAdminAsync(Admin admin)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, admin.AdminId.ToString()),
            new(ClaimTypes.Name, admin.FullName),
            new(ClaimTypes.Email, admin.Email),
            new(ClaimTypes.Role, "Admin")
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
