using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.Dtos.RequestDtos;

public class BookCreateDto
{
    [Required(ErrorMessage = "Book name is required.")]
    [StringLength(100)]
    public string BookName { get; set; } = null!;

    [Required(ErrorMessage = "Auther name is required.")]
    [StringLength(100)]
    public string AutherName { get; set; } = null!;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Total copies is required.")]
    [Range(1, 1000, ErrorMessage = "Total copies must be between 1 and 1000.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Total copies must be a whole number.")]
    public int TotalCopies { get; set; }

    [Required(ErrorMessage = "PublisherId is required.")]
    public long PublisherId { get; set; }
}
