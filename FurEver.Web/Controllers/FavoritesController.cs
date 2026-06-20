using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FurEver.Web.Data;
using FurEver.Web.Models;

namespace FurEver.Web.Controllers;

[Authorize(Roles = "Adopter")]
public class FavoritesController : Controller
{
    private readonly FurEverContext _db;

    public FavoritesController(FurEverContext db) => _db = db;

    private int AdopterId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /Favorites
    public async Task<IActionResult> Index()
    {
        var favorites = await _db.Favorites
            .Where(f => f.AdopterId == AdopterId)
            .Include(f => f.Pet)
            .OrderByDescending(f => f.DateAdded)
            .ToListAsync();

        return View(favorites);
    }

    // POST /Favorites/Add
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int petId)
    {
        var exists = await _db.Favorites.AnyAsync(f => f.AdopterId == AdopterId && f.PetId == petId);
        if (!exists)
        {
            _db.Favorites.Add(new Favorite { AdopterId = AdopterId, PetId = petId });
            await _db.SaveChangesAsync();

            // Signal the pet page to pop up the "add a note" modal for the
            // pet that was just favorited.
            TempData["JustFavorited"] = petId;
        }

        return RedirectToAction("Details", "Pets", new { id = petId });
    }

    // POST /Favorites/Remove
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id, string? returnTo)
    {
        var favorite = await _db.Favorites
            .FirstOrDefaultAsync(f => f.FavoriteId == id && f.AdopterId == AdopterId);

        if (favorite is not null)
        {
            var petId = favorite.PetId;
            _db.Favorites.Remove(favorite);
            await _db.SaveChangesAsync();

            if (returnTo == "pet")
                return RedirectToAction("Details", "Pets", new { id = petId });
        }

        return RedirectToAction(nameof(Index));
    }

    // POST /Favorites/UpdateNotes
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateNotes(int id, string? notes, string? returnTo, int? petId)
    {
        var favorite = await _db.Favorites
            .FirstOrDefaultAsync(f => f.FavoriteId == id && f.AdopterId == AdopterId);

        if (favorite is not null)
        {
            favorite.Notes = notes;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Note saved.";
        }

        if (returnTo == "pet" && petId.HasValue)
            return RedirectToAction("Details", "Pets", new { id = petId.Value });

        return RedirectToAction(nameof(Index));
    }
}
