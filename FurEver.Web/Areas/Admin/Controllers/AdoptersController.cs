using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FurEver.Web.Data;
using FurEver.Web.Models.ViewModels;

namespace FurEver.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AdoptersController : Controller
{
    private readonly FurEverContext _db;

    public AdoptersController(FurEverContext db) => _db = db;

    // GET /Admin/Adopters
    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Adopters.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.FullName.Contains(search) || a.Email.Contains(search));

        var rows = await query
            .OrderBy(a => a.FullName)
            .Select(a => new AdminAdopterRow
            {
                Adopter = a,
                AdoptionCount = a.Adoptions.Count,
                FavoriteCount = a.Favorites.Count
            })
            .ToListAsync();

        ViewBag.Search = search;
        return View(rows);
    }

    // GET /Admin/Adopters/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var adopter = await _db.Adopters
            .Include(a => a.Adoptions).ThenInclude(ad => ad.Pet)
            .Include(a => a.Favorites).ThenInclude(f => f.Pet)
            .FirstOrDefaultAsync(a => a.AdopterId == id);

        if (adopter is null) return NotFound();
        return View(adopter);
    }

    // GET /Admin/Adopters/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var adopter = await _db.Adopters.FindAsync(id);
        if (adopter is null) return NotFound();

        return View(new AdminAdopterEditViewModel
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

    // POST /Admin/Adopters/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminAdopterEditViewModel model)
    {
        var adopter = await _db.Adopters.FindAsync(id);
        if (adopter is null) return NotFound();

        if (!ModelState.IsValid) return View(model);

        // Guard against assigning an email already used by another adopter.
        var emailTaken = await _db.Adopters
            .AnyAsync(a => a.Email == model.Email && a.AdopterId != id);
        if (emailTaken)
        {
            ModelState.AddModelError(nameof(model.Email), "Another account already uses this email.");
            return View(model);
        }

        adopter.Email = model.Email;
        adopter.FullName = model.FullName;
        adopter.ContactNo = model.ContactNo;
        adopter.Address = model.Address;
        adopter.HousingType = model.HousingType;
        adopter.HasOtherPets = model.HasOtherPets;
        adopter.HasChildren = model.HasChildren;
        adopter.ExperienceLevel = model.ExperienceLevel;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Updated {adopter.FullName}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST /Admin/Adopters/Delete/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var adopter = await _db.Adopters
            .Include(a => a.Adoptions)
            .FirstOrDefaultAsync(a => a.AdopterId == id);
        if (adopter is null) return NotFound();

        var name = adopter.FullName;

        // Remove the adoptions explicitly so EF issues a DELETE per row. A raw
        // FK cascade would drop the rows without firing trg_adoption_after_delete,
        // leaving the adopter's pets stuck as Reserved/Adopted. Deleting the
        // adoption rows individually fires the trigger, freeing those pets back
        // to Available. Favorites are still removed by the FK cascade.
        _db.Adoptions.RemoveRange(adopter.Adoptions);
        _db.Adopters.Remove(adopter);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Deleted {name} and their records.";
        return RedirectToAction(nameof(Index));
    }
}
