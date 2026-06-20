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

    public async Task<IActionResult> Index(int? year, int? month)
    {
        var today = DateTime.Today;
        var model = new ReportsViewModel
        {
            Year = year ?? today.Year,
            Month = month ?? today.Month
        };

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

        model.AllAdoptions = await _db.Adoptions
            .Include(a => a.Pet)
            .Include(a => a.Adopter)
            .OrderByDescending(a => a.ApplicationDate)
            .ThenByDescending(a => a.AdoptionId)
            .ToListAsync();

        var completedFees = await _db.Adoptions
            .Where(a => a.Status == "Completed" && a.AdoptionFee != null)
            .Select(a => a.AdoptionFee!.Value)
            .ToListAsync();
        model.AverageFee = completedFees.Count > 0 ? completedFees.Average() : 0m;

        return View(model);
    }
}
