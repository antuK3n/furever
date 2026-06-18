using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FurEver.Web.Models;

[Table("Favorite")]
public class Favorite
{
    [Key]
    [Column("Favorite_ID")]
    public int FavoriteId { get; set; }

    [Required]
    [Column("Adopter_ID")]
    public int AdopterId { get; set; }

    [Required]
    [Column("Pet_ID")]
    public int PetId { get; set; }

    [Column("Date_Added")]
    [Display(Name = "Date Added")]
    public DateTime DateAdded { get; set; } = DateTime.Now;

    public string? Notes { get; set; }

    [ForeignKey(nameof(AdopterId))]
    public Adopter? Adopter { get; set; }

    [ForeignKey(nameof(PetId))]
    public Pet? Pet { get; set; }
}
