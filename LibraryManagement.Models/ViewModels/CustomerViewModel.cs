using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.ViewModels;

public class CustomerViewModel
{
    [Key]
    public long Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(100)]
    public string Email { get; set; } = null!;

}
