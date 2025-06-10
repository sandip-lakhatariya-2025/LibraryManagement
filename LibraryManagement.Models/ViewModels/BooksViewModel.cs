using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.ViewModels;

public class BooksViewModel
{
    public long Id { get; set; }

    [StringLength(100)]
    public string BookName { get; set; } = null!;

    [StringLength(100)]
    public string AutherName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int TotalCopies { get; set; }

    public int PublisherId { get; set; }

    public PublisherViewModel? PublisherDetails { get; set; }

    public ReaderDetailsViewModel? ReaderDetails { get; set; }
}
