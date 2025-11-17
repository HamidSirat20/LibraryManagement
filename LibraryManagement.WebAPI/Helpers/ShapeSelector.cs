using System.Dynamic;
using System.Reflection;

namespace LibraryManagement.WebAPI.Helpers;
public static class ShapeSelector
{
    public static IEnumerable<ExpandoObject> ShapeFields<TSource>(this IEnumerable<TSource> source, string? fields)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        //dynamic obj
        var expandoObjectList = new List<ExpandoObject>();

        //dynamic obj's properties
        var propertyInfoList = new List<PropertyInfo>();

        if (string.IsNullOrWhiteSpace(fields))
        {
            var propertyInfos = typeof(TSource)
                .GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            propertyInfoList.AddRange(propertyInfos);
        }
        else
        {
            var fieldsAfterSplit = fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var field in fieldsAfterSplit)
            {
                var propertyInfo = typeof(TSource).GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo == null)
                {
                    throw new Exception($"Property {field} wasn't found on {typeof(TSource)}");
                }
                propertyInfoList.Add(propertyInfo);
            }
        }

        foreach (TSource sourceObj in source)
        {
            var shapedObj = new ExpandoObject();

            foreach (var propertyInfo in propertyInfoList)
            {
                var propertyValue = propertyInfo.GetValue(sourceObj);
                ((IDictionary<string, object?>)shapedObj)
                    .Add(propertyInfo.Name, propertyValue);
            }
            expandoObjectList.Add(shapedObj);
        }
        return expandoObjectList;
    }

}