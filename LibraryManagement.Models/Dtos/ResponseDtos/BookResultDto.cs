namespace LibraryManagement.Models.Dtos.ResponseDtos;

public class BookResultDto
{
    public long Id { get; set; }
    public string BookName { get; set; } = null!;
    public string AutherName { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int TotalCopies { get; set; }
    public long PublisherId { get; set; }
    public DateTime CreatedAt { get; set; }
}
