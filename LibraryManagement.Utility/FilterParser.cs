using System.Linq.Expressions;
using LibraryManagement.Models.Enums;
using LibraryManagement.Models.Models;
using LibraryManagement.Models.ViewModels;
using Microsoft.AspNetCore.Http;

namespace LibraryManagement.Utility;

public static class FilterParser
{
    public static List<FilterCriteria> ParseFilters(IQueryCollection? query)
    {
        var filters = new List<FilterCriteria>();
        if (query == null) return filters;

        var reservedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "PageNumber", "PageSize", "SortField", "isascending", "SearchTerm", "Fields"
        };

        foreach (var param in query)
        {
            var key = param.Key;
            var value = param.Value.ToString();

            bool isOr = key.Contains("[or]");
            key = key.Replace("[or]", "");

            string[] parts = key.Split('.');

            if (reservedKeys.Contains(parts[0])){
                continue;
            }

            var propertyPath = parts.SkipLast(1);
            string op = parts.Length > 1 ? parts.Last() : "eq";

            if (Enum.TryParse<FilterOperator>(op, true, out var filterOperator))
            {
                filters.Add(new FilterCriteria
                {
                    PropertyPath = propertyPath,
                    Operator = filterOperator,
                    Value = value,
                    IsOrCondition = isOr
                });
            }
        }

        return filters;
    }


    public static IQueryable<T> ApplyFilters<T>(IQueryable<T> source, List<FilterCriteria> filters)
    {
        if (filters == null || !filters.Any())
            return source;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combined = null;

        foreach (var filter in filters)
        {
            Expression property = filter.PropertyPath.Aggregate((Expression)parameter, Expression.PropertyOrField);

            object? convertedValue = Convert.ChangeType(filter.Value, property.Type);
            var constant = Expression.Constant(convertedValue);

            Expression comparison = filter.Operator switch
            {
                FilterOperator.Eq => Expression.Equal(property, constant),
                FilterOperator.Neq => Expression.NotEqual(property, constant),
                FilterOperator.Gt => Expression.GreaterThan(property, constant),
                FilterOperator.Gteq => Expression.GreaterThanOrEqual(property, constant),
                FilterOperator.Lt => Expression.LessThan(property, constant),
                FilterOperator.Lteq => Expression.LessThanOrEqual(property, constant),
                FilterOperator.Sw => Expression.Call(property, typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!, constant),
                FilterOperator.Ew => Expression.Call(property, typeof(string).GetMethod("EndsWith", new[] { typeof(string) })!, constant),
                FilterOperator.Like => Expression.Call(property, typeof(string).GetMethod("Contains", new[] { typeof(string) })!, constant),
                _ => throw new NotSupportedException($"Unsupported operator: {filter.Operator}")
            };

            if (combined == null)
            {
                combined = comparison;
            }
            else
            {
                combined = filter.IsOrCondition
                    ? Expression.OrElse(combined, comparison)
                    : Expression.AndAlso(combined, comparison);
            }
        }

        var lambda = Expression.Lambda<Func<T, bool>>(combined!, parameter);
        return source.Where(lambda);
    }
}
