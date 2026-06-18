using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FurEver.Web.Models;

[Table("Adopter")]
public class Adopter
{
    [Key]
    [Column("Adopter_ID")]
    public int AdopterId { get; set; }

    [Required, StringLength(100), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(255)]
    [Column("Password_Hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Column("Full_Name")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(15)]
    [Column("Contact_No")]
    [Display(Name = "Contact Number")]
    public string ContactNo { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [Column("Housing_Type")]
    [Display(Name = "Housing Type")]
    public string HousingType { get; set; } = "House";

    [Required, StringLength(3)]
    [Column("Has_Other_Pets")]
    [Display(Name = "Has Other Pets")]
    public string HasOtherPets { get; set; } = "No";

    [Required, StringLength(3)]
    [Column("Has_Children")]
    [Display(Name = "Has Children")]
    public string HasChildren { get; set; } = "No";

    [Required, StringLength(20)]
    [Column("Experience_Level")]
    [Display(Name = "Experience Level")]
    public string ExperienceLevel { get; set; } = "First-time";

    public ICollection<Adoption> Adoptions { get; set; } = new List<Adoption>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
