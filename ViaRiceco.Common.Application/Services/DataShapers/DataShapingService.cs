using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;

namespace ViaRiceco.Common.Application.Services.DataShapers;

public class DataShapingService : IDataShapingService
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

    public ExpandoObject ShapeData<T>(T entity, string? fields = null)
    {
        HashSet<string> fieldsSet = fields?
            .Split(',')
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

        PropertyInfo[] propertyInfos = _propertyCache.GetOrAdd(typeof(T),
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        if (fieldsSet.Any())
        {
            propertyInfos = [.. propertyInfos.Where(p => fieldsSet.Contains(p.Name))];
        }
        
        IDictionary<string, object?> shapedObject = new ExpandoObject();
        
        foreach (PropertyInfo propertyInfo in propertyInfos)
        {
            shapedObject[propertyInfo.Name] = propertyInfo.GetValue(entity);
        }
        
        return (ExpandoObject)shapedObject;
    }
    
    public ExpandoObject ShapeData<T>(T entity, Hyperlink[] hyperlinks)
    {
        PropertyInfo[] propertyInfos = _propertyCache.GetOrAdd(typeof(T),
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        IDictionary<string, object?> shapedObject = new ExpandoObject();
        
        foreach (PropertyInfo propertyInfo in propertyInfos)
        {
            shapedObject[propertyInfo.Name] = propertyInfo.GetValue(entity);
        }
        
        shapedObject["_links"] = hyperlinks;
        
        return (ExpandoObject)shapedObject;
    }

    public bool Validate<T>(string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
        {
            return true;
        }

        var fieldsSet = fields
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        PropertyInfo[] propertyInfos = _propertyCache.GetOrAdd(typeof(T),
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        return fieldsSet.All(f => propertyInfos.Any(p => p.Name.Equals(f, StringComparison.OrdinalIgnoreCase)));
    }
}
