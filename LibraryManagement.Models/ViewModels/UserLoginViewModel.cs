using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.ViewModels;

public class UserLoginViewModel
{
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Required(ErrorMessage = "Email is required.")]
    [MaxLength(100, ErrorMessage = "Email must be less than or equal to 100 characters.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    [MaxLength(20, ErrorMessage = "Password must be less than or equal to 20 characters.")]
    public string Password { get; set; } = null!;
}
