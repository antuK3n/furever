using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FurEver.Web.Models;

[Table("Adoption")]
public class Adoption
{
    [Key]
    [Column("Adoption_ID")]
    public int AdoptionId { get; set; }

    [Required]
    [Column("Pet_ID")]
    public int PetId { get; set; }

    [Required]
    [Column("Adopter_ID")]
    public int AdopterId { get; set; }

    [Column("Application_Date")]
    [DataType(DataType.Date)]
    [Display(Name = "Application Date")]
    public DateOnly ApplicationDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Column("Adoption_Date")]
    [DataType(DataType.Date)]
    [Display(Name = "Adoption Date")]
    public DateOnly? AdoptionDate { get; set; }

    [Column("Adoption_Fee", TypeName = "decimal(10,2)")]
    [Display(Name = "Adoption Fee")]
    public decimal? AdoptionFee { get; set; }

    [Required, StringLength(3)]
    [Column("Contract_Signed")]
    [Display(Name = "Contract Signed")]
    public string ContractSigned { get; set; } = "No";

    [Required, StringLength(20)]
    public string Status { get; set; } = "Pending";

    [ForeignKey(nameof(PetId))]
    public Pet? Pet { get; set; }

    [ForeignKey(nameof(AdopterId))]
    public Adopter? Adopter { get; set; }
}
