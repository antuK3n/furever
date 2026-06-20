namespace FurEver.Web.Models.ViewModels;

public class HomeViewModel
{
    public List<PetWithCount> PopularPets { get; set; } = new();
    public List<Pet> NewArrivals { get; set; } = new();

    public List<Pet> AvailablePets { get; set; } = new();
}

public class PetWithCount
{
    public Pet Pet { get; set; } = null!;
    public int FavoriteCount { get; set; }
}

public class PetListViewModel
{
    public List<Pet> Pets { get; set; } = new();
    public List<string> SpeciesOptions { get; set; } = new();
    public string? Species { get; set; }
    public string? Search { get; set; }
    public string? Status { get; set; }
}

public class PetDetailViewModel
{
    public Pet Pet { get; set; } = null!;
    public List<VeterinaryVisit> VetHistory { get; set; } = new();
    public bool IsFavorited { get; set; }
    public int? FavoriteId { get; set; }
    public bool HasPendingApplication { get; set; }
}
