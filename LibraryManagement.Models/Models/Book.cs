using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.Models;

public class Book
{
    [Key]
    public long Id { get; set; }

    [Required]
    [StringLength(100)]
    public string BookName { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string AutherName { get; set; } = null!;

    public string? Description { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    public int TotalCopies { get; set; }

    [Required]
    public long PublisherId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Publisher Publisher { get; set; } = null!;

    public virtual ICollection<CustomerBook> CustomerBooks { get; set; } = new List<CustomerBook>();
}
