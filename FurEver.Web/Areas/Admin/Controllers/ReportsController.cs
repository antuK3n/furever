using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FurEver.Web.Data;
using FurEver.Web.Models.ViewModels;

namespace FurEver.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ReportsController : Controller
{
    private readonly FurEverContext _db;

    public ReportsController(FurEverContext db) => _db = db;

    // GET /Admin/Reports?year=2026&month=6
    public async Task<IActionResult> Index(int? year, int? month)
    {
        var today = DateTime.Today;
        var model = new ReportsViewModel
        {
            Year = year ?? today.Year,
            Month = month ?? today.Month
        };

        // Monthly stats via the stored procedure (ported from MySQL).
        var stats = await _db.Database
            .SqlQuery<MonthlyAdoptionStats>(
                $"EXEC dbo.sp_monthly_adoption_stats @p_year = {model.Year}, @p_month = {model.Month}")
            .ToListAsync();
        model.Stats = stats.FirstOrDefault();

        model.SpeciesBreakdown = await _db.Pets
            .GroupBy(p => p.Species)
            .Select(g => new SpeciesCount
            {
                Species = g.Key,
                Available = g.Count(p => p.Status == "Available"),
                Total = g.Count()
            })
            .OrderByDescending(s => s.Total)
            .ToListAsync();

        return View(model);
    }
}
