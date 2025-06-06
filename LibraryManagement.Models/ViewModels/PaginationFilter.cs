namespace LibraryManagement.Models.ViewModels;

public class PaginationFilter
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
    public string? SortField { get; set; } = "Id";
    public bool IsAscending { get; set; } = true;
    public string? SearchTerm { get; set; }
}
