using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FurEver.Web.Data;
using FurEver.Web.Models.ViewModels;

namespace FurEver.Web.Controllers;

public class PetsController : Controller
{
    private readonly FurEverContext _db;

    public PetsController(FurEverContext db) => _db = db;

    // GET /Pets
    // Defaults to "Available" pets. Visitors can opt into other statuses
    // (Reserved / Adopted / Medical Hold) or "All" by changing the filter.
    public async Task<IActionResult> Index(string? species, string? search, string? status)
    {
        var effectiveStatus = string.IsNullOrWhiteSpace(status) ? "Available" : status;

        var query = _db.Pets.AsQueryable();

        if (effectiveStatus != "All")
            query = query.Where(p => p.Status == effectiveStatus);

        if (!string.IsNullOrWhiteSpace(species))
            query = query.Where(p => p.Species == species);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.PetName.Contains(search) ||
                p.Breed.Contains(search) ||
                p.Temperament.Contains(search));

        var pets = await query
            .OrderByDescending(p => p.DateArrived)
            .ToListAsync();

        var speciesOptions = await _db.Pets
            .Select(p => p.Species)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();

        return View(new PetListViewModel
        {
            Pets = pets,
            SpeciesOptions = speciesOptions,
            Species = species,
            Search = search,
            Status = effectiveStatus
        });
    }

    // GET /Pets/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var pet = await _db.Pets.FindAsync(id);
        if (pet is null) return NotFound();

        var vetHistory = await _db.VetVisits
            .Where(v => v.PetId == id)
            .Include(v => v.Vaccinations)
            .OrderByDescending(v => v.VisitDate)
            .ToListAsync();

        var model = new PetDetailViewModel
        {
            Pet = pet,
            VetHistory = vetHistory
        };

        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Adopter"))
        {
            var adopterId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var favorite = await _db.Favorites
                .FirstOrDefaultAsync(f => f.AdopterId == adopterId && f.PetId == id);
            model.IsFavorited = favorite is not null;
            model.FavoriteId = favorite?.FavoriteId;

            model.HasPendingApplication = await _db.Adoptions.AnyAsync(a =>
                a.AdopterId == adopterId && a.PetId == id && a.Status == "Pending");
        }

        return View(model);
    }
}
