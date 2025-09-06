using System.Dynamic;
using ViaRiceco.Common.Application.Services.DataShapers.Helpers;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;

namespace ViaRiceco.Common.Application.Services.DataShapers;

public sealed class DataShapingService : IDataShapingService
{
    public ExpandoObject ShapeData<T>(T entity, string? fields = null, Hyperlink[]? hyperlinks = null)
    {
        HashSet<string> requestedFields = FieldParser.ParseFields(fields);
        
        PropertyAccessor<T>[] allAccessors = PropertyAccessor<T>.GetAccessors();
        PropertyAccessor<T>[] filteredAccessors = PropertyAccessor<T>.FilterAccessors(allAccessors, requestedFields);
        
        return DataShaper<T>.ShapeEntity(entity, filteredAccessors, hyperlinks);
    }
    
    public IReadOnlyCollection<ExpandoObject> ShapeCollectionData<T>(IReadOnlyCollection<T> entities, string? fields = null, Func<T, Hyperlink[]>? generateLinks = null)
    {
        if (entities.Count == 0)
        {
            return [];
        }

        HashSet<string> requestedFields = FieldParser.ParseFields(fields);
        
        PropertyAccessor<T>[] allAccessors = PropertyAccessor<T>.GetAccessors();
        PropertyAccessor<T>[] filteredAccessors = PropertyAccessor<T>.FilterAccessors(allAccessors, requestedFields);
        
        return DataShaper<T>.ShapeCollection(entities, filteredAccessors, generateLinks);
    }

    public bool Validate<T>(string? fields)
    {
        HashSet<string> requestedFields = FieldParser.ParseFields(fields);
        
        PropertyAccessor<T>[] accessors = PropertyAccessor<T>.GetAccessors();
        
        return FieldParser.ValidateFields(requestedFields, accessors);
    }
}
