using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.ViewModels;

public class PublisherViewModel
{
    public long Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(200)]
    public string? Address { get; set; }
}
