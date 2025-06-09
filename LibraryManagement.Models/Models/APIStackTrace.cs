namespace LibraryManagement.Models.Models;

public class APIStackTrace
{
    public int Id { get; set; }
    public string EndpointName { get; set; } = string.Empty;
    public string RequestRoute { get; set; } = string.Empty;
    public string ApiType { get; set; } = string.Empty;
    public string? ErrorLog { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
