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

        // Subquery 2: newest arrivals (top 5 by arrival date)
        var newArrivals = await _db.Pets
            .Where(p => p.Status == "Available")
            .OrderByDescending(p => p.DateArrived)
            .ThenByDescending(p => p.PetId)
            .Take(5)
            .ToListAsync();

        return View(new HomeViewModel
        {
            PopularPets = popular,
            NewArrivals = newArrivals
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
