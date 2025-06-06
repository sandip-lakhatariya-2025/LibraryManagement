using System.Dynamic;
using System.Reflection;

namespace LibraryManagement.Utility;
// public static class DataShaper
// {
//     public static List<ExpandoObject> ShapeData<T>(IEnumerable<T> data, string? fields)
//     {
//         var shapedData = new List<ExpandoObject>();

//         if (data == null) return shapedData;

//         var allProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

//         var selectedProps = string.IsNullOrWhiteSpace(fields)
//             ? allProperties
//             : allProperties.Where(p => fields
//                 .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
//                 .Contains(p.Name, StringComparer.OrdinalIgnoreCase)).ToArray();

//         foreach (var item in data)
//         {
//             var shapedObj = new ExpandoObject() as IDictionary<string, object?>;

//             foreach (var prop in selectedProps)
//             {
//                 shapedObj[prop.Name] = prop.GetValue(item);
//             }

//             shapedData.Add((ExpandoObject)shapedObj);
//         }

//         return shapedData;
//     }

// }

public static class ObjectShaper
{
    public static ExpandoObject GetShapedObject<T>(T entity, string? fields)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (string.IsNullOrWhiteSpace(fields))
            return ToExpando(entity); // Full object

        var expando = new ExpandoObject() as IDictionary<string, object?>;

        var fieldPaths = fields
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        var grouped = fieldPaths.GroupBy(p => p.Contains('.') ? p[..p.IndexOf('.')] : p, StringComparer.OrdinalIgnoreCase);

        foreach (var group in grouped)
        {
            string topPropName = group.Key;
            var propInfo = entity.GetType().GetProperty(topPropName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (propInfo == null) continue;

            var value = propInfo.GetValue(entity);
            if (value == null) continue;

            if (group.Count() == 1 && group.First().Equals(topPropName, StringComparison.OrdinalIgnoreCase))
            {
                expando[topPropName] = value;
            }
            else
            {
                var nestedFields = group.Select(p => p[(p.IndexOf('.') + 1)..]);
                if (value is IEnumerable<object> collection)
                {
                    var list = new List<ExpandoObject>();
                    foreach (var item in collection)
                    {
                        list.Add(GetShapedObject(item, string.Join(",", nestedFields)));
                    }
                    expando[topPropName] = list;
                }
                else
                {
                    expando[topPropName] = GetShapedObject(value, string.Join(",", nestedFields));
                }
            }
        }

        return (ExpandoObject)expando;
    }

    private static ExpandoObject ToExpando(object obj)
    {
        var expando = new ExpandoObject() as IDictionary<string, object?>;
        foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = prop.GetValue(obj);
            expando[prop.Name] = value;
        }
        return (ExpandoObject)expando;
    }
}
