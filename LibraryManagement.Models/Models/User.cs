using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Email { get; set; } = null!;

    [Required]
    public string Firstname { get; set; } = null!;

    public string? Lastname { get; set; }

    [Required]
    public string Password { get; set; } = null!;

    [Required]
    public string Role { get; set; } = null!;
}
