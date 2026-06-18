using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FurEver.Web.Models;

[Table("Vaccination")]
public class Vaccination
{
    [Key]
    [Column("Vaccination_ID")]
    public int VaccinationId { get; set; }

    [Required]
    [Column("Visit_ID")]
    public int VisitId { get; set; }

    [Required, StringLength(100)]
    [Column("Vaccine_Name")]
    [Display(Name = "Vaccine")]
    public string VaccineName { get; set; } = string.Empty;

    [Column("Date_Administered")]
    [DataType(DataType.Date)]
    [Display(Name = "Date Administered")]
    public DateOnly? DateAdministered { get; set; }

    [StringLength(100)]
    [Column("Administered_By")]
    [Display(Name = "Administered By")]
    public string? AdministeredBy { get; set; }

    [StringLength(50)]
    public string? Manufacturer { get; set; }

    [Column("Next_Due_Date")]
    [DataType(DataType.Date)]
    [Display(Name = "Next Due")]
    public DateOnly? NextDueDate { get; set; }

    [StringLength(50)]
    public string? Site { get; set; }

    public string? Reaction { get; set; }

    [Required, StringLength(20)]
    public string Status { get; set; } = "Scheduled";

    [Column(TypeName = "decimal(10,2)")]
    public decimal? Cost { get; set; }

    [ForeignKey(nameof(VisitId))]
    public VeterinaryVisit? Visit { get; set; }
}
