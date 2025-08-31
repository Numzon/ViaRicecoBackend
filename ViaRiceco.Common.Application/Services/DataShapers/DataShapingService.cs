using System.Dynamic;
using ViaRiceco.Common.Application.Services.DataShapers.Helpers;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;

namespace ViaRiceco.Common.Application.Services.DataShapers;

/// <summary>
/// High-performance data shaping service with compiled property access and optimized caching
/// </summary>
public sealed class DataShapingService : IDataShapingService
{
    public ExpandoObject ShapeData<T>(T entity, string? fields = null, Hyperlink[]? hyperlinks = null)
    {
        // Parse and cache field filtering
        HashSet<string> requestedFields = FieldParser.ParseFields(fields);
        
        // Get compiled property accessors (cached per type)
        PropertyAccessor<T>[] allAccessors = PropertyAccessor<T>.GetAccessors();
        PropertyAccessor<T>[] filteredAccessors = PropertyAccessor<T>.FilterAccessors(allAccessors, requestedFields);
        
        // Shape entity using optimized core logic
        return DataShaper<T>.ShapeEntity(entity, filteredAccessors, hyperlinks);
    }
    
    public IReadOnlyCollection<ExpandoObject> ShapeCollectionData<T>(IReadOnlyCollection<T> entities, string? fields = null, Func<T, Hyperlink[]>? generateLinks = null)
    {
        // Early return for empty collections
        if (entities.Count == 0)
        {
            return [];
        }

        // Parse and cache field filtering (shared across all entities)
        HashSet<string> requestedFields = FieldParser.ParseFields(fields);
        
        // Get compiled property accessors (cached per type, computed once per collection)
        PropertyAccessor<T>[] allAccessors = PropertyAccessor<T>.GetAccessors();
        PropertyAccessor<T>[] filteredAccessors = PropertyAccessor<T>.FilterAccessors(allAccessors, requestedFields);
        
        // Shape collection using optimized core logic with pre-allocated capacity
        return DataShaper<T>.ShapeCollection(entities, filteredAccessors, generateLinks);
    }

    public bool Validate<T>(string? fields)
    {
        // Parse fields using cached parser
        HashSet<string> requestedFields = FieldParser.ParseFields(fields);
        
        // Get property accessors for validation
        PropertyAccessor<T>[] accessors = PropertyAccessor<T>.GetAccessors();
        
        // Use optimized validation logic
        return FieldParser.ValidateFields(requestedFields, accessors);
    }
}
