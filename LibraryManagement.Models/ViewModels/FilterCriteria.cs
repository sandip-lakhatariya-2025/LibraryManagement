using System.Linq.Expressions;
using LibraryManagement.Models.Enums;
using LibraryManagement.Models.Models;

namespace LibraryManagement.Models.ViewModels;

public class FilterCriteria
{
    // public string? PropertyName { get; set; }
    // public Expression PropertyName { get; set; }
    public IEnumerable<string> PropertyPath { get; set; } = Enumerable.Empty<string>();
    public FilterOperator Operator { get; set; }
    public string? Value { get; set; }
    public bool IsOrCondition { get; set; }
}