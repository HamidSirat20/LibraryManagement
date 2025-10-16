using System.Dynamic;
using System.Reflection;

namespace LibraryManagement.WebAPI.Helpers;

   public static class ShapeSingleObj
   {
    public static ExpandoObject ShapeField<TSource>(this TSource source, string? fields)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        //dynamic obj
        var expandoObject = new ExpandoObject();

        if (string.IsNullOrWhiteSpace(fields))
        {
            var propertyInfos = typeof(TSource)
                .GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            foreach (var propertyInfo in propertyInfos)
            {
                var propertyValue = propertyInfo.GetValue(source);
                ((IDictionary<string, object?>)expandoObject)
                    .Add(propertyInfo.Name, propertyValue);
            }
            return expandoObject;
        }
       
            var fieldsAfterSplit = fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
           
            foreach (var field in fieldsAfterSplit)
            {
                var propertyInfo = typeof(TSource).GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo == null)
                {
                    throw new Exception($"Property {field} wasn't found on {typeof(TSource)}");
                }
                var propertyValue = propertyInfo.GetValue(source);
                ((IDictionary<string, object?>)expandoObject)
                    .Add(propertyInfo.Name, propertyValue);
            }
       

        

        return expandoObject;
    }
    }

