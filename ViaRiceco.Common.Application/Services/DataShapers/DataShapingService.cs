using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;
using ViaRiceco.Common.Application.Abstractions.Collections;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;

namespace ViaRiceco.Common.Application.Services.DataShapers;

public sealed class DataShapingService : IDataShapingService
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();
    
    public ExpandoObject ShapeData<T>(T entity, string? fields = null, Hyperlink[]? hyperlinks = null)
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
        
        shapedObject["_links"] = hyperlinks;
        
        return (ExpandoObject)shapedObject;
    }
    
    public IReadOnlyCollection<ExpandoObject> ShapeCollectionData<T>(IReadOnlyCollection<T> entities, string? fields = null, Func<T, Hyperlink[]>? generateLinks = null)
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

        List<ExpandoObject> shapedObjects = [];
        foreach (T entity in entities)
        {
            IDictionary<string, object?> shapedObject = new ExpandoObject();
        
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                shapedObject[propertyInfo.Name] = propertyInfo.GetValue(entity);
            }
        
            if (generateLinks is not null)
            {
                shapedObject["_links"] = generateLinks(entity);
            }
            
            shapedObjects.Add((ExpandoObject)shapedObject);
        }
        
        return shapedObjects;
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
