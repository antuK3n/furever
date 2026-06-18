using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FurEver.Web.Data;
using FurEver.Web.Models;

namespace FurEver.Web.Controllers;

[Authorize(Roles = "Adopter")]
public class AdoptionsController : Controller
{
    private readonly FurEverContext _db;

    public AdoptionsController(FurEverContext db) => _db = db;

    private int AdopterId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /Adoptions — my applications
    public async Task<IActionResult> Index()
    {
        var adoptions = await _db.Adoptions
            .Where(a => a.AdopterId == AdopterId)
            .Include(a => a.Pet)
            .OrderByDescending(a => a.ApplicationDate)
            .ToListAsync();

        return View(adoptions);
    }

    // POST /Adoptions/Apply
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(int petId)
    {
        var pet = await _db.Pets.FindAsync(petId);
        if (pet is null) return NotFound();

        if (pet.Status != "Available")
        {
            TempData["Error"] = "This pet is no longer available for adoption.";
            return RedirectToAction("Details", "Pets", new { id = petId });
        }

        var alreadyApplied = await _db.Adoptions.AnyAsync(a =>
            a.AdopterId == AdopterId && a.PetId == petId && a.Status == "Pending");

        if (alreadyApplied)
        {
            TempData["Error"] = "You already have a pending application for this pet.";
            return RedirectToAction("Details", "Pets", new { id = petId });
        }

        _db.Adoptions.Add(new Adoption
        {
            PetId = petId,
            AdopterId = AdopterId,
            ApplicationDate = DateOnly.FromDateTime(DateTime.Today),
            ContractSigned = "No",
            Status = "Pending"
        });

        try
        {
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Application submitted for {pet.PetName}! We'll review it shortly.";
        }
        catch (DbUpdateException)
        {
            // Trigger rejects non-available pets (race condition safety net)
            TempData["Error"] = "This pet is no longer available for adoption.";
        }

        return RedirectToAction("Details", "Pets", new { id = petId });
    }

    // POST /Adoptions/Cancel — adopter cancels own pending application
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var adoption = await _db.Adoptions
            .FirstOrDefaultAsync(a => a.AdoptionId == id && a.AdopterId == AdopterId);

        if (adoption is null) return NotFound();

        if (adoption.Status != "Pending")
        {
            TempData["Error"] = "Only pending applications can be cancelled.";
            return RedirectToAction(nameof(Index));
        }

        adoption.Status = "Cancelled";
        await _db.SaveChangesAsync();

        TempData["Success"] = "Application cancelled.";
        return RedirectToAction(nameof(Index));
    }
}
