using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FurEver.Web.Data;
using FurEver.Web.Models.ViewModels;

namespace FurEver.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly FurEverContext _db;

    public DashboardController(FurEverContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var model = new DashboardViewModel
        {
            AvailablePets = await _db.Pets.CountAsync(p => p.Status == "Available"),
            ReservedPets = await _db.Pets.CountAsync(p => p.Status == "Reserved"),
            AdoptedPets = await _db.Pets.CountAsync(p => p.Status == "Adopted"),
            MedicalHoldPets = await _db.Pets.CountAsync(p => p.Status == "Medical Hold"),
            TotalAdopters = await _db.Adopters.CountAsync(),
            PendingApplications = await _db.Adoptions.CountAsync(a => a.Status == "Pending"),
            OverdueVaccinations = await _db.Vaccinations.CountAsync(v => v.Status == "Overdue"),
            RecentApplications = await _db.Adoptions
                .Include(a => a.Pet)
                .Include(a => a.Adopter)
                .OrderByDescending(a => a.ApplicationDate)
                .ThenByDescending(a => a.AdoptionId)
                .Take(5)
                .ToListAsync(),
            UpcomingVisits = await _db.VetVisits
                .Include(v => v.Pet)
                .Where(v => v.NextVisitDate != null && v.NextVisitDate >= today)
                .OrderBy(v => v.NextVisitDate)
                .Take(5)
                .ToListAsync()
        };

        return View(model);
    }
}
