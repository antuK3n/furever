using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FurEver.Web.Data;
using FurEver.Web.Models.ViewModels;

namespace FurEver.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class PetsController : Controller
{
    private const long MaxPhotoBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly string[] AllowedPhotoTypes =
        { "image/jpeg", "image/png", "image/gif", "image/webp" };

    private readonly FurEverContext _db;
    private readonly IWebHostEnvironment _env;

    public PetsController(FurEverContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    // GET /Admin/Pets
    public async Task<IActionResult> Index(string? species, string? status, string? search)
    {
        var query = _db.Pets.AsQueryable();

        if (!string.IsNullOrWhiteSpace(species))
            query = query.Where(p => p.Species == species);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status == status);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.PetName.Contains(search) || p.Breed.Contains(search));

        var model = new PetListViewModel
        {
            Pets = await query.OrderByDescending(p => p.DateArrived).ThenByDescending(p => p.PetId).ToListAsync(),
            SpeciesOptions = await _db.Pets.Select(p => p.Species).Distinct().OrderBy(s => s).ToListAsync(),
            Species = species,
            Status = status,
            Search = search
        };

        return View(model);
    }

    // GET /Admin/Pets/Create
    [HttpGet]
    public IActionResult Create() => View(new PetFormViewModel());

    // POST /Admin/Pets/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PetFormViewModel model)
    {
        ValidatePhoto(model.Photo);
        if (!ModelState.IsValid) return View(model);

        if (model.Photo is not null)
            model.Pet.PhotoUrl = await SavePhotoAsync(model.Photo);

        _db.Pets.Add(model.Pet);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"{model.Pet.PetName} added.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Admin/Pets/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var pet = await _db.Pets.FindAsync(id);
        if (pet is null) return NotFound();

        return View(new PetFormViewModel { Pet = pet });
    }

    // POST /Admin/Pets/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PetFormViewModel model)
    {
        var pet = await _db.Pets.FindAsync(id);
        if (pet is null) return NotFound();

        ValidatePhoto(model.Photo);
        if (!ModelState.IsValid)
        {
            model.Pet.PetId = id;
            model.Pet.PhotoUrl = pet.PhotoUrl;
            return View(model);
        }

        pet.PetName = model.Pet.PetName;
        pet.Species = model.Pet.Species;
        pet.Breed = model.Pet.Breed;
        pet.Age = model.Pet.Age;
        pet.Gender = model.Pet.Gender;
        pet.Color = model.Pet.Color;
        pet.DateArrived = model.Pet.DateArrived;
        pet.SpayedNeutered = model.Pet.SpayedNeutered;
        pet.Temperament = model.Pet.Temperament;
        pet.SpecialNeeds = model.Pet.SpecialNeeds;
        pet.Status = model.Pet.Status;

        if (model.Photo is not null)
        {
            DeletePhoto(pet.PhotoUrl);
            pet.PhotoUrl = await SavePhotoAsync(model.Photo);
        }

        await _db.SaveChangesAsync();

        TempData["Success"] = $"{pet.PetName} updated.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Admin/Pets/Delete/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var pet = await _db.Pets.FindAsync(id);
        if (pet is null) return NotFound();

        DeletePhoto(pet.PhotoUrl);
        _db.Pets.Remove(pet);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"{pet.PetName} removed.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- Photo helpers ----------

    private void ValidatePhoto(IFormFile? photo)
    {
        if (photo is null) return;

        if (photo.Length > MaxPhotoBytes)
            ModelState.AddModelError("Photo", "Photo must be 5 MB or smaller.");
        if (!AllowedPhotoTypes.Contains(photo.ContentType))
            ModelState.AddModelError("Photo", "Photo must be a JPEG, PNG, GIF, or WebP image.");
    }

    private async Task<string> SavePhotoAsync(IFormFile photo)
    {
        var uploads = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploads);

        var ext = photo.ContentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            _ => ".webp"
        };
        var fileName = $"{Guid.NewGuid():N}{ext}";

        await using var stream = System.IO.File.Create(Path.Combine(uploads, fileName));
        await photo.CopyToAsync(stream);

        return $"/uploads/{fileName}";
    }

    private void DeletePhoto(string? photoUrl)
    {
        if (string.IsNullOrEmpty(photoUrl) || !photoUrl.StartsWith("/uploads/")) return;

        var path = Path.Combine(_env.WebRootPath, "uploads", Path.GetFileName(photoUrl));
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
    }
}
