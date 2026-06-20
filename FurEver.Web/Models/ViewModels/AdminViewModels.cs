using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FurEver.Web.Models.ViewModels;

public class DashboardViewModel
{
    public int AvailablePets { get; set; }
    public int ReservedPets { get; set; }
    public int AdoptedPets { get; set; }
    public int MedicalHoldPets { get; set; }
    public int TotalAdopters { get; set; }
    public int PendingApplications { get; set; }
    public int OverdueVaccinations { get; set; }
    public List<Adoption> RecentApplications { get; set; } = new();
    public List<VeterinaryVisit> UpcomingVisits { get; set; } = new();
}

public class PetFormViewModel
{
    public Pet Pet { get; set; } = new();
    public IFormFile? Photo { get; set; }
}

public class AdminAdoptionListViewModel
{
    public List<Adoption> Adoptions { get; set; } = new();
    public string? Status { get; set; }

    public int TotalCount { get; set; }
    public int PendingCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }
    public int ReturnedCount { get; set; }
}

public class AdminAdopterRow
{
    public Adopter Adopter { get; set; } = null!;
    public int AdoptionCount { get; set; }
    public int FavoriteCount { get; set; }
}

public class AdminAdopterEditViewModel
{
    public int AdopterId { get; set; }

    [Required, StringLength(100), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(15)]
    [Display(Name = "Contact Number")]
    public string ContactNo { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Housing Type")]
    public string HousingType { get; set; } = "House";

    [Required]
    [Display(Name = "Has Other Pets")]
    public string HasOtherPets { get; set; } = "No";

    [Required]
    [Display(Name = "Has Children")]
    public string HasChildren { get; set; } = "No";

    [Required]
    [Display(Name = "Experience Level")]
    public string ExperienceLevel { get; set; } = "First-time";
}

public class AdminVetVisitListViewModel
{
    public List<VeterinaryVisit> Visits { get; set; } = new();
    public string? VisitType { get; set; }
    public int? PetId { get; set; }
    public List<Pet> PetOptions { get; set; } = new();
}

public class MonthlyAdoptionStats
{
    [Column("Total_Adoptions")]
    public int TotalAdoptions { get; set; }

    public int? Completed { get; set; }
    public int? Pending { get; set; }
    public int? Cancelled { get; set; }
    public int? Returned { get; set; }

    [Column("Total_Revenue")]
    public decimal TotalRevenue { get; set; }
}

public class ReportsViewModel
{
    [Range(2000, 2100)]
    public int Year { get; set; }

    [Range(1, 12)]
    public int Month { get; set; }

    public MonthlyAdoptionStats? Stats { get; set; }
    public List<SpeciesCount> SpeciesBreakdown { get; set; } = new();

    public List<Adoption> AllAdoptions { get; set; } = new();

    public decimal AverageFee { get; set; }
}

public class SpeciesCount
{
    public string Species { get; set; } = string.Empty;
    public int Available { get; set; }
    public int Total { get; set; }
}
