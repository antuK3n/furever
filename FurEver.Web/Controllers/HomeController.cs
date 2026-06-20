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

        var cutoff = DateOnly.FromDateTime(DateTime.Today).AddDays(-30);
        var newArrivals = await _db.Pets
            .Where(p => p.Status == "Available" && p.DateArrived >= cutoff)
            .OrderByDescending(p => p.DateArrived)
            .ThenByDescending(p => p.PetId)
            .Take(5)
            .ToListAsync();

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
