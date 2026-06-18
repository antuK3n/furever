using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FurEver.Web.Models;

[Table("Pet")]
public class Pet
{
    [Key]
    [Column("Pet_ID")]
    public int PetId { get; set; }

    [Required, StringLength(50)]
    [Column("Pet_Name")]
    [Display(Name = "Name")]
    public string PetName { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Species { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Breed { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Age { get; set; } = string.Empty;

    [Required, StringLength(10)]
    public string Gender { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Color { get; set; } = string.Empty;

    [Required]
    [Column("Date_Arrived")]
    [DataType(DataType.Date)]
    [Display(Name = "Date Arrived")]
    public DateOnly DateArrived { get; set; }

    [Required, StringLength(3)]
    [Column("Spayed_Neutered")]
    [Display(Name = "Spayed / Neutered")]
    public string SpayedNeutered { get; set; } = "No";

    [Required, StringLength(100)]
    public string Temperament { get; set; } = string.Empty;

    [Column("Special_Needs")]
    [Display(Name = "Special Needs")]
    public string? SpecialNeeds { get; set; }

    [StringLength(255)]
    [Column("Photo_URL")]
    public string? PhotoUrl { get; set; }

    [Required, StringLength(20)]
    public string Status { get; set; } = "Available";

    public ICollection<Adoption> Adoptions { get; set; } = new List<Adoption>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<VeterinaryVisit> VetVisits { get; set; } = new List<VeterinaryVisit>();
}
