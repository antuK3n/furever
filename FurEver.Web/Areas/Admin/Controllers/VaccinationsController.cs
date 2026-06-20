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

    public async Task<IActionResult> Index(string? status)
    {
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

    [HttpGet]
    public async Task<IActionResult> Create(int visitId)
    {
        var visit = await _db.VetVisits.Include(v => v.Pet).FirstOrDefaultAsync(v => v.VisitId == visitId);
        if (visit is null) return NotFound();

        ViewBag.Visit = visit;
        return View(new Vaccination { VisitId = visitId });
    }

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

    [HttpGet]
    public async Task<IActionResult> Edit(int id, string? returnUrl)
    {
        var vaccination = await _db.Vaccinations
            .Include(v => v.Visit).ThenInclude(visit => visit!.Pet)
            .FirstOrDefaultAsync(v => v.VaccinationId == id);
        if (vaccination is null) return NotFound();

        ViewBag.Visit = vaccination.Visit;
        ViewBag.ReturnUrl = returnUrl;
        return View(vaccination);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Vaccination model, string? returnUrl)
    {
        var vaccination = await _db.Vaccinations.FindAsync(id);
        if (vaccination is null) return NotFound();

        ModelState.Remove(nameof(model.Visit));
        if (!ModelState.IsValid)
        {
            ViewBag.Visit = await _db.VetVisits.Include(v => v.Pet)
                .FirstOrDefaultAsync(v => v.VisitId == vaccination.VisitId);
            ViewBag.ReturnUrl = returnUrl;
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

        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Details", "VetVisits", new { id = vaccination.VisitId });
    }

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
