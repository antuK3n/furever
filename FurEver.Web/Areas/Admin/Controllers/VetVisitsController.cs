using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FurEver.Web.Data;
using FurEver.Web.Models;
using FurEver.Web.Models.ViewModels;

namespace FurEver.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class VetVisitsController : Controller
{
    private readonly FurEverContext _db;

    public VetVisitsController(FurEverContext db) => _db = db;

    // GET /Admin/VetVisits
    public async Task<IActionResult> Index(string? visitType, int? petId)
    {
        var query = _db.VetVisits.Include(v => v.Pet).AsQueryable();

        if (!string.IsNullOrWhiteSpace(visitType))
            query = query.Where(v => v.VisitType == visitType);
        if (petId.HasValue)
            query = query.Where(v => v.PetId == petId);

        var model = new AdminVetVisitListViewModel
        {
            Visits = await query
                .OrderByDescending(v => v.VisitDate)
                .ThenByDescending(v => v.VisitId)
                .ToListAsync(),
            VisitType = visitType,
            PetId = petId,
            PetOptions = await _db.Pets.OrderBy(p => p.PetName).ToListAsync()
        };

        return View(model);
    }

    // GET /Admin/VetVisits/Details/5 — visit + its vaccinations
    public async Task<IActionResult> Details(int id)
    {
        var visit = await _db.VetVisits
            .Include(v => v.Pet)
            .Include(v => v.Vaccinations)
            .FirstOrDefaultAsync(v => v.VisitId == id);

        if (visit is null) return NotFound();
        return View(visit);
    }

    // GET /Admin/VetVisits/Create
    [HttpGet]
    public async Task<IActionResult> Create(int? petId)
    {
        await LoadPetOptionsAsync();
        return View(new VeterinaryVisit
        {
            PetId = petId ?? 0,
            VisitDate = DateOnly.FromDateTime(DateTime.Today)
        });
    }

    // POST /Admin/VetVisits/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VeterinaryVisit visit)
    {
        ModelState.Remove(nameof(visit.Pet));
        if (!ModelState.IsValid)
        {
            await LoadPetOptionsAsync();
            return View(visit);
        }

        _db.VetVisits.Add(visit);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Vet visit recorded.";
        return RedirectToAction(nameof(Details), new { id = visit.VisitId });
    }

    // GET /Admin/VetVisits/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var visit = await _db.VetVisits.FindAsync(id);
        if (visit is null) return NotFound();

        await LoadPetOptionsAsync();
        return View(visit);
    }

    // POST /Admin/VetVisits/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VeterinaryVisit model)
    {
        var visit = await _db.VetVisits.FindAsync(id);
        if (visit is null) return NotFound();

        ModelState.Remove(nameof(model.Pet));
        if (!ModelState.IsValid)
        {
            await LoadPetOptionsAsync();
            model.VisitId = id;
            return View(model);
        }

        visit.PetId = model.PetId;
        visit.VisitDate = model.VisitDate;
        visit.VeterinarianName = model.VeterinarianName;
        visit.VisitType = model.VisitType;
        visit.Weight = model.Weight;
        visit.Temperature = model.Temperature;
        visit.Diagnosis = model.Diagnosis;
        visit.GeneralNotes = model.GeneralNotes;
        visit.ProcedureCost = model.ProcedureCost;
        visit.NextVisitDate = model.NextVisitDate;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Vet visit updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST /Admin/VetVisits/Delete/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var visit = await _db.VetVisits.FindAsync(id);
        if (visit is null) return NotFound();

        _db.VetVisits.Remove(visit);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Vet visit deleted (vaccinations included).";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadPetOptionsAsync() =>
        ViewBag.PetOptions = await _db.Pets.OrderBy(p => p.PetName).ToListAsync();
}
