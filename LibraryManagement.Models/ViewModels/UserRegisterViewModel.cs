using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.ViewModels;

public class UserRegisterViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(100, ErrorMessage = "Email must be less than or equal to 100 characters.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "First name is required.")]
    [MaxLength(50, ErrorMessage = "First name must be less than or equal to 50 characters.")]
    public string Firstname { get; set; } = null!;

    [MaxLength(50, ErrorMessage = "Last name must be less than or equal to 50 characters.")]
    public string? Lastname { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    [MaxLength(20, ErrorMessage = "Password must be less than or equal to 20 characters.")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Role is required.")]
    public string Role { get; set; } = null!;
}
