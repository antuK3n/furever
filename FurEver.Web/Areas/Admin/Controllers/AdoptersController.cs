using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FurEver.Web.Data;

namespace FurEver.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AdoptersController : Controller
{
    private readonly FurEverContext _db;

    public AdoptersController(FurEverContext db) => _db = db;

    // GET /Admin/Adopters
    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Adopters.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.FullName.Contains(search) || a.Email.Contains(search));

        ViewBag.Search = search;
        return View(await query.OrderBy(a => a.FullName).ToListAsync());
    }

    // GET /Admin/Adopters/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var adopter = await _db.Adopters
            .Include(a => a.Adoptions).ThenInclude(ad => ad.Pet)
            .Include(a => a.Favorites).ThenInclude(f => f.Pet)
            .FirstOrDefaultAsync(a => a.AdopterId == id);

        if (adopter is null) return NotFound();
        return View(adopter);
    }
}
