using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models.Models;

public class Publisher
{
    [Key]
    public long Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(200)]
    public string? Address { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
