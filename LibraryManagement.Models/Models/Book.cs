using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.Models;

public class Book
{
    [Key]
    public long Id { get; set; }

    [StringLength(100)]
    public string BookName { get; set; } = null!;

    [StringLength(100)]
    public string AutherName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int TotalCopies { get; set; }

    public long PublisherId { get; set; }

    public virtual Publisher Publisher { get; set; } = null!;

    public virtual ICollection<CustomerBook> CustomerBooks { get; set; } = new List<CustomerBook>();
}
