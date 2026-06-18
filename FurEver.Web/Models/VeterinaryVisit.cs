using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FurEver.Web.Models;

[Table("Veterinary_Visit")]
public class VeterinaryVisit
{
    [Key]
    [Column("Visit_ID")]
    public int VisitId { get; set; }

    [Required]
    [Column("Pet_ID")]
    public int PetId { get; set; }

    [Required]
    [Column("Visit_Date")]
    [DataType(DataType.Date)]
    [Display(Name = "Visit Date")]
    public DateOnly VisitDate { get; set; }

    [Required, StringLength(100)]
    [Column("Veterinarian_Name")]
    [Display(Name = "Veterinarian")]
    public string VeterinarianName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [Column("Visit_Type")]
    [Display(Name = "Visit Type")]
    public string VisitType { get; set; } = "Checkup";

    [Column(TypeName = "decimal(5,2)")]
    [Display(Name = "Weight (kg)")]
    public decimal? Weight { get; set; }

    [Column(TypeName = "decimal(4,2)")]
    [Display(Name = "Temperature (°C)")]
    public decimal? Temperature { get; set; }

    public string? Diagnosis { get; set; }

    [Column("General_Notes")]
    [Display(Name = "Notes")]
    public string? GeneralNotes { get; set; }

    [Required]
    [Column("Procedure_Cost", TypeName = "decimal(10,2)")]
    [Display(Name = "Procedure Cost")]
    public decimal ProcedureCost { get; set; }

    [Column("Next_Visit_Date")]
    [DataType(DataType.Date)]
    [Display(Name = "Next Visit")]
    public DateOnly? NextVisitDate { get; set; }

    [ForeignKey(nameof(PetId))]
    public Pet? Pet { get; set; }

    public ICollection<Vaccination> Vaccinations { get; set; } = new List<Vaccination>();
}
