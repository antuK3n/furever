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
}

public class AdminVetVisitListViewModel
{
    public List<VeterinaryVisit> Visits { get; set; } = new();
    public string? VisitType { get; set; }
    public int? PetId { get; set; }
    public List<Pet> PetOptions { get; set; } = new();
}

// Maps the result set of dbo.sp_monthly_adoption_stats.
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
}

public class SpeciesCount
{
    public string Species { get; set; } = string.Empty;
    public int Available { get; set; }
    public int Total { get; set; }
}
