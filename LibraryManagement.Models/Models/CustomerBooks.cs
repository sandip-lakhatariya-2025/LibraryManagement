namespace LibraryManagement.Models.Models;

public class CustomerBook
{
    public long CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;

    public long BookId { get; set; }
    public virtual Book Book { get; set; } = null!;
}

