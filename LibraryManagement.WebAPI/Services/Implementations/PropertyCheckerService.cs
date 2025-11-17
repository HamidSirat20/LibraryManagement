using LibraryManagement.WebAPI.Services.Interfaces;
using System.Reflection;

namespace LibraryManagement.WebAPI.Services.Implementations;
public class PropertyCheckerService : IPropertyCheckerService
{
    public bool TypeHasProperties<T>(string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
        {
            return true;
        }
        var fieldsAfterSplit = fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var field in fieldsAfterSplit)
        {
            var propertyInfo = typeof(T).GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo == null)
            {
                return false;
            }
        }
        return true;
    }
}

