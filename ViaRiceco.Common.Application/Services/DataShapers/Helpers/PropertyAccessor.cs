using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace ViaRiceco.Common.Application.Services.DataShapers.Helpers;

internal sealed class PropertyAccessor<T>
{
    private static readonly ConcurrentDictionary<Type, PropertyAccessor<T>[]> AccessorCache = new();
    
    public string PropertyName { get; }
    public Func<T, object?> GetValue { get; }

    private PropertyAccessor(PropertyInfo propertyInfo)
    {
        PropertyName = propertyInfo.Name;
        GetValue = CreateCompiledAccessor(propertyInfo);
    }

    public static PropertyAccessor<T>[] GetAccessors()
    {
        return AccessorCache.GetOrAdd(typeof(T), static type =>
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(prop => new PropertyAccessor<T>(prop))
                .ToArray();
        });
    }

    public static PropertyAccessor<T>[] FilterAccessors(PropertyAccessor<T>[] accessors, HashSet<string> requestedFields)
    {
        if (requestedFields.Count == 0)
        {
            return accessors;
        }

        return accessors.Where(accessor => requestedFields.Contains(accessor.PropertyName))
                        .ToArray();
    }

    private static Func<T, object?> CreateCompiledAccessor(PropertyInfo propertyInfo)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "entity");
        MemberExpression propertyAccess = Expression.Property(parameter, propertyInfo);
        UnaryExpression convertToObject = Expression.Convert(propertyAccess, typeof(object));
        
        var lambda = Expression.Lambda<Func<T, object?>>(convertToObject, parameter);
        
        return lambda.Compile();
    }
}
