using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FurEver.Web.Data;
using FurEver.Web.Models;

namespace FurEver.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class VaccinationsController : Controller
{
    private readonly FurEverContext _db;

    public VaccinationsController(FurEverContext db) => _db = db;

    // GET /Admin/Vaccinations — overview, overdue first
    public async Task<IActionResult> Index(string? status)
    {
        // Keep statuses fresh (replaces the MySQL daily event).
        await _db.Database.ExecuteSqlRawAsync("EXEC dbo.sp_update_overdue_vaccinations");

        var query = _db.Vaccinations
            .Include(v => v.Visit).ThenInclude(visit => visit!.Pet)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(v => v.Status == status);

        ViewBag.Status = status;
        return View(await query
            .OrderBy(v => v.Status == "Overdue" ? 0 : v.Status == "Scheduled" ? 1 : 2)
            .ThenBy(v => v.NextDueDate)
            .ToListAsync());
    }

    // GET /Admin/Vaccinations/Create?visitId=5
    [HttpGet]
    public async Task<IActionResult> Create(int visitId)
    {
        var visit = await _db.VetVisits.Include(v => v.Pet).FirstOrDefaultAsync(v => v.VisitId == visitId);
        if (visit is null) return NotFound();

        ViewBag.Visit = visit;
        return View(new Vaccination { VisitId = visitId });
    }

    // POST /Admin/Vaccinations/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Vaccination vaccination)
    {
        ModelState.Remove(nameof(vaccination.Visit));
        if (!ModelState.IsValid)
        {
            ViewBag.Visit = await _db.VetVisits.Include(v => v.Pet)
                .FirstOrDefaultAsync(v => v.VisitId == vaccination.VisitId);
            return View(vaccination);
        }

        _db.Vaccinations.Add(vaccination);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Vaccination recorded.";
        return RedirectToAction("Details", "VetVisits", new { id = vaccination.VisitId });
    }

    // GET /Admin/Vaccinations/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var vaccination = await _db.Vaccinations
            .Include(v => v.Visit).ThenInclude(visit => visit!.Pet)
            .FirstOrDefaultAsync(v => v.VaccinationId == id);
        if (vaccination is null) return NotFound();

        ViewBag.Visit = vaccination.Visit;
        return View(vaccination);
    }

    // POST /Admin/Vaccinations/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Vaccination model)
    {
        var vaccination = await _db.Vaccinations.FindAsync(id);
        if (vaccination is null) return NotFound();

        ModelState.Remove(nameof(model.Visit));
        if (!ModelState.IsValid)
        {
            ViewBag.Visit = await _db.VetVisits.Include(v => v.Pet)
                .FirstOrDefaultAsync(v => v.VisitId == vaccination.VisitId);
            model.VaccinationId = id;
            return View(model);
        }

        vaccination.VaccineName = model.VaccineName;
        vaccination.DateAdministered = model.DateAdministered;
        vaccination.AdministeredBy = model.AdministeredBy;
        vaccination.Manufacturer = model.Manufacturer;
        vaccination.NextDueDate = model.NextDueDate;
        vaccination.Site = model.Site;
        vaccination.Reaction = model.Reaction;
        vaccination.Status = model.Status;
        vaccination.Cost = model.Cost;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Vaccination updated.";
        return RedirectToAction("Details", "VetVisits", new { id = vaccination.VisitId });
    }

    // POST /Admin/Vaccinations/Delete/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var vaccination = await _db.Vaccinations.FindAsync(id);
        if (vaccination is null) return NotFound();

        var visitId = vaccination.VisitId;
        _db.Vaccinations.Remove(vaccination);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Vaccination deleted.";
        return RedirectToAction("Details", "VetVisits", new { id = visitId });
    }
}
