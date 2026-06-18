using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FurEver.Web.Data;
using FurEver.Web.Models.ViewModels;

namespace FurEver.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AdoptionsController : Controller
{
    private readonly FurEverContext _db;

    public AdoptionsController(FurEverContext db) => _db = db;

    // GET /Admin/Adoptions
    public async Task<IActionResult> Index(string? status)
    {
        var query = _db.Adoptions
            .Include(a => a.Pet)
            .Include(a => a.Adopter)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(a => a.Status == status);

        var model = new AdminAdoptionListViewModel
        {
            Adoptions = await query
                .OrderByDescending(a => a.ApplicationDate)
                .ThenByDescending(a => a.AdoptionId)
                .ToListAsync(),
            Status = status
        };

        return View(model);
    }

    // POST /Admin/Adoptions/Approve — Pending -> Completed.
    // The DB trigger fills Adoption_Date + Contract_Signed and
    // marks the pet Adopted (which also clears favorites).
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, decimal fee)
    {
        var adoption = await _db.Adoptions.Include(a => a.Pet).FirstOrDefaultAsync(a => a.AdoptionId == id);
        if (adoption is null) return NotFound();

        if (adoption.Status != "Pending")
        {
            TempData["Error"] = "Only pending applications can be approved.";
            return RedirectToAction(nameof(Index));
        }

        if (fee < 0)
        {
            TempData["Error"] = "Adoption fee cannot be negative.";
            return RedirectToAction(nameof(Index));
        }

        adoption.Status = "Completed";
        adoption.AdoptionFee = fee;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Adoption completed — {adoption.Pet?.PetName} has a new home!";
        return RedirectToAction(nameof(Index));
    }

    // POST /Admin/Adoptions/Reject — Pending -> Cancelled (frees the pet).
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var adoption = await _db.Adoptions.FirstOrDefaultAsync(a => a.AdoptionId == id);
        if (adoption is null) return NotFound();

        if (adoption.Status != "Pending")
        {
            TempData["Error"] = "Only pending applications can be rejected.";
            return RedirectToAction(nameof(Index));
        }

        adoption.Status = "Cancelled";
        await _db.SaveChangesAsync();

        TempData["Success"] = "Application rejected.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Admin/Adoptions/Return — Completed -> Returned (frees the pet).
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(int id)
    {
        var adoption = await _db.Adoptions.FirstOrDefaultAsync(a => a.AdoptionId == id);
        if (adoption is null) return NotFound();

        if (adoption.Status != "Completed")
        {
            TempData["Error"] = "Only completed adoptions can be marked as returned.";
            return RedirectToAction(nameof(Index));
        }

        adoption.Status = "Returned";
        await _db.SaveChangesAsync();

        TempData["Success"] = "Adoption marked as returned. The pet is available again.";
        return RedirectToAction(nameof(Index));
    }
}
