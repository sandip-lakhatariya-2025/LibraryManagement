using LibraryManagement.Models.Enums;

namespace LibraryManagement.Models.ViewModels;

public class FilterCriteria
{
    public string? PropertyName { get; set; }
    public FilterOperator Operator { get; set; }
    public string? Value { get; set; }
    public bool IsOrCondition { get; set; }
}