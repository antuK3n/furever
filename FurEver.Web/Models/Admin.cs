using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FurEver.Web.Models;

[Table("Admin")]
public class Admin
{
    [Key]
    [Column("Admin_ID")]
    public int AdminId { get; set; }

    [Required, StringLength(255), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(255)]
    [Column("Password_Hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [Column("Full_Name")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;
}
