using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FurEver.Web.Data;

namespace FurEver.Web.Controllers;

public class VetVisitsController : Controller
{
    private readonly FurEverContext _db;

    public VetVisitsController(FurEverContext db) => _db = db;

    public async Task<IActionResult> Index(string? visitType)
    {
        var query = _db.VetVisits
            .Include(v => v.Pet)
            .Include(v => v.Vaccinations)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(visitType))
            query = query.Where(v => v.VisitType == visitType);

        var visits = await query
            .OrderByDescending(v => v.VisitDate)
            .Take(100)
            .ToListAsync();

        ViewBag.VisitType = visitType;
        return View(visits);
    }
}
