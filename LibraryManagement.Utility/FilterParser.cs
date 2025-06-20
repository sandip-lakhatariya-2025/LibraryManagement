using System.Globalization;
using System.Linq.Expressions;
using LibraryManagement.Models.Enums;
using LibraryManagement.Models.Models;
using LibraryManagement.Models.ViewModels;
using Microsoft.AspNetCore.Http;

namespace LibraryManagement.Utility;

public static class FilterParser
{
    private static readonly Dictionary<string, FilterOperator> OperatorMappings = new()
    {
        { "==", FilterOperator.Eq },
        { "!=", FilterOperator.Neq },
        { ">", FilterOperator.Gt },
        { ">=", FilterOperator.Gteq },
        { "<", FilterOperator.Lt },
        { "<=", FilterOperator.Lteq },
        { "Contains", FilterOperator.Like },
        { "StartsWith", FilterOperator.Sw },
        { "EndsWith", FilterOperator.Ew }
    };

    public static List<FilterCriteria> ParseFiltersV1(IQueryCollection? query)
    {
        var filters = new List<FilterCriteria>();
        if (query == null) return filters;

        var reservedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "PageNumber", "PageSize", "SortField", "isascending", "SearchTerm", "Fields"
        };

        HashSet<string> validOperators = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
        { 
            "Eq", "Gt", "Lteq", "Lt", "Gteq", "Neq", "Sw", "Like", "Ew"
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

            string lastPart = parts.Last();
            string op;
            IEnumerable<string> propertyPath;

            if (validOperators.Contains(lastPart)) {
                op = lastPart;
                propertyPath = parts.Take(parts.Length - 1);
            } else {
                op = "Eq";
                propertyPath = parts; 
            }

            if (propertyPath.Last().Equals("createdAt", StringComparison.OrdinalIgnoreCase))
            {
                if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                {
                    value = dt.ToString("o");
                }
            }

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

    public static IQueryable<T> ApplyFiltersV1<T>(IQueryable<T> source, List<FilterCriteria> filters)
    {
        if (filters == null || !filters.Any())
            return source;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combined = null;

        foreach (var filter in filters)
        {
            Expression property = filter.PropertyPath.Aggregate((Expression)parameter, Expression.PropertyOrField);

            object? convertedValue;

            if (property.Type == typeof(DateTime) || property.Type == typeof(DateTime?))
            {
                convertedValue = DateTime.Parse(filter.Value!, null, DateTimeStyles.RoundtripKind);
            }
            else
            {
                convertedValue = Convert.ChangeType(filter.Value, property.Type);
            }

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

    public static List<List<FilterCriteria>> ParseFiltersV2(string? sFilters)
    {
        var result = new List<List<FilterCriteria>>();

        if (string.IsNullOrEmpty(sFilters))
            return result;

        var andParts = sFilters.Split("&&", StringSplitOptions.RemoveEmptyEntries);

        foreach (var andPart in andParts)
        {
            var orParts = andPart.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
            var orGroup = new List<FilterCriteria>();

            foreach (var orPart in orParts)
            {
                var trimmedPart = orPart.Trim();

                var op = OperatorMappings.Keys.OrderByDescending(k => k.Length)
                    .FirstOrDefault(o => trimmedPart.Contains(o));
                if (op == null) continue;

                var split = trimmedPart.Split(new[] { op }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length != 2) continue;

                var prop = split[0].Trim();
                var val = split[1].Trim().Trim('"');
                var propPath = prop.Split('.').ToList();

                if (propPath.Last().Equals("createdAt", StringComparison.OrdinalIgnoreCase)
                    && DateTime.TryParse(val, CultureInfo.InvariantCulture,
                       DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                {
                    val = dt.ToString("o");
                }

                orGroup.Add(new FilterCriteria
                {
                    PropertyPath = propPath,
                    Operator = OperatorMappings[op],
                    Value = val
                });
            }

            if (orGroup.Any())
                result.Add(orGroup);
        }

        return result;
    }

    public static IQueryable<T> ApplyFiltersV2<T>(IQueryable<T> source, List<List<FilterCriteria>> groupedFilters)
    {
        if (groupedFilters == null || !groupedFilters.Any())
            return source;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combinedAnd = null;

        foreach (var orGroup in groupedFilters)
        {
            Expression? combinedOr = null;

            foreach (var filter in orGroup)
            {
                Expression property = filter.PropertyPath.Aggregate((Expression)parameter, Expression.PropertyOrField);

                object? convertedValue;

                if (property.Type == typeof(DateTime) || property.Type == typeof(DateTime?))
                {
                    convertedValue = DateTime.Parse(filter.Value!, null, DateTimeStyles.RoundtripKind);
                }
                else
                {
                    convertedValue = Convert.ChangeType(filter.Value, property.Type);
                }

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

                combinedOr = combinedOr == null ? comparison : Expression.OrElse(combinedOr, comparison);
            }

            if (combinedOr != null)
            {
                combinedAnd = combinedAnd == null ? combinedOr : Expression.AndAlso(combinedAnd, combinedOr);
            }
        }

        var lambda = Expression.Lambda<Func<T, bool>>(combinedAnd!, parameter);
        return source.Where(lambda);
    }
}

// public static List<FilterCriteria> ParseFiltersV2(string? sFilters)
// {
//     List<FilterCriteria> lstFilters = new List<FilterCriteria>();

//     if(string.IsNullOrEmpty(sFilters)) {
//         return lstFilters;
//     }

//     var andParts = sFilters!.Split(',', StringSplitOptions.RemoveEmptyEntries);

//     foreach (var andPart in andParts)
//     {
//         var orParts = andPart.Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

//         foreach (var orPart in orParts)
//         {
//             var trimmedPart = orPart.Trim();

//             var op = OperatorMappings.Keys.OrderByDescending(k => k.Length)
//                 .FirstOrDefault(o => trimmedPart.Contains(o));

//             if (op == null) continue;

//             var split = trimmedPart.Split(new[] { op }, 2, StringSplitOptions.RemoveEmptyEntries);
//             if (split.Length != 2) continue;

//             var prop = split[0].Trim();
//             var val = split[1].Trim().Trim('"');

//             var propPath = prop.Split('.').ToList();

//             if (propPath.Last().Equals("createdAt", StringComparison.OrdinalIgnoreCase))
//             {
//                 if (DateTime.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
//                 {
//                     val = dt.ToString("o");
//                 }
//             }

//             bool isOrCondition = orPart != orParts[0] && orParts.Length > 1;

//             lstFilters.Add(new FilterCriteria
//             {
//                 PropertyPath = propPath,
//                 Operator = OperatorMappings[op],
//                 Value = val,
//                 IsOrCondition = isOrCondition
//             });
//         }
//     }

//     return lstFilters;
// }