using System.ComponentModel.DataAnnotations;

namespace FurEver.Web.Models.ViewModels;

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

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
    [Display(Name = "Do you have other pets?")]
    public string HasOtherPets { get; set; } = "No";

    [Required]
    [Display(Name = "Do you have children?")]
    public string HasChildren { get; set; } = "No";

    [Required]
    [Display(Name = "Experience Level")]
    public string ExperienceLevel { get; set; } = "First-time";
}

public class ProfileViewModel
{
    public int AdopterId { get; set; }

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

    public string Email { get; set; } = string.Empty;
}
