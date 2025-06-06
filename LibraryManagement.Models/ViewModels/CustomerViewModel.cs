using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.ViewModels;

public class CustomerViewModel
{
    [Key]
    public long Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(100)]
    public string Email { get; set; } = null!;

}
