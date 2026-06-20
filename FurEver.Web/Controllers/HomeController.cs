using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FurEver.Web.Data;
using FurEver.Web.Models;
using FurEver.Web.Models.ViewModels;

namespace FurEver.Web.Controllers;

public class HomeController : Controller
{
    private readonly FurEverContext _db;

    public HomeController(FurEverContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        // Subquery 1: top 10 most-favorited available pets
        var popular = await _db.Pets
            .Where(p => p.Status == "Available")
            .Select(p => new PetWithCount
            {
                Pet = p,
                FavoriteCount = p.Favorites.Count
            })
            .Where(x => x.FavoriteCount > 0)
            .OrderByDescending(x => x.FavoriteCount)
            .ThenBy(x => x.Pet.PetName)
            .Take(10)
            .ToListAsync();

        // Subquery 2: newest arrivals — available pets that arrived within the
        // last 30 days, newest first (capped at 5). The date window keeps the
        // "New arrivals" label honest: pets stop appearing once they're no
        // longer recent, rather than always showing the 5 latest regardless of age.
        var cutoff = DateOnly.FromDateTime(DateTime.Today).AddDays(-30);
        var newArrivals = await _db.Pets
            .Where(p => p.Status == "Available" && p.DateArrived >= cutoff)
            .OrderByDescending(p => p.DateArrived)
            .ThenByDescending(p => p.PetId)
            .Take(5)
            .ToListAsync();

        // Fallback: when nothing is favorited yet and there are no recent
        // arrivals, still showcase available pets so the home page is never
        // "empty" while adoptable pets exist.
        var availablePets = new List<Pet>();
        if (popular.Count == 0 && newArrivals.Count == 0)
        {
            availablePets = await _db.Pets
                .Where(p => p.Status == "Available")
                .OrderByDescending(p => p.DateArrived)
                .ThenByDescending(p => p.PetId)
                .Take(8)
                .ToListAsync();
        }

        return View(new HomeViewModel
        {
            PopularPets = popular,
            NewArrivals = newArrivals,
            AvailablePets = availablePets
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
