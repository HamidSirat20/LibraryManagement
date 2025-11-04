using System.Linq.Expressions;
using System.Reflection;

namespace LibraryManagement.WebAPI.Helpers;
    public static class SortedResponse
    {
        public static IQueryable<T> ApplySorting<T>(
            this IQueryable<T> query,
            string sortBy,
            bool isDescending,
            string defaultSortProperty)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                sortBy = defaultSortProperty;

            var entityType = typeof(T);
            var property = entityType.GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
            {
                
                property = entityType.GetProperty(defaultSortProperty, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                {
                    return query;
                }
            }

            var parameter = Expression.Parameter(entityType, "x");
            var propertyAccess = Expression.Property(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);

            string methodName = isDescending ? "OrderByDescending" : "OrderBy";

            var resultExp = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { entityType, property.PropertyType },
                query.Expression,
                Expression.Quote(orderByExp));

            return query.Provider.CreateQuery<T>(resultExp);
        }
    }

