using System.Dynamic;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;

namespace ViaRiceco.Common.Application.Services.DataShapers.Helpers;

internal static class DataShaper<T>
{
    public static ExpandoObject ShapeEntity(T entity, PropertyAccessor<T>[] accessors, Hyperlink[]? hyperlinks = null)
    {
        var shapedObject = new ExpandoObject();
        IDictionary<string, object?> dictionary = shapedObject;

        foreach (PropertyAccessor<T> accessor in accessors)
        {
            dictionary[accessor.PropertyName] = accessor.GetValue(entity);
        }
        
        if (hyperlinks is not null)
        {
            dictionary["_links"] = hyperlinks;
        }

        return shapedObject;
    }

    public static IReadOnlyCollection<ExpandoObject> ShapeCollection(
        IReadOnlyCollection<T> entities,
        PropertyAccessor<T>[] accessors,
        Func<T, Hyperlink[]>? generateLinks = null)
    {
        var shapedObjects = new List<ExpandoObject>(entities.Count);

        foreach (T entity in entities)
        {
            Hyperlink[]? hyperlinks = generateLinks?.Invoke(entity);
            shapedObjects.Add(ShapeEntity(entity, accessors, hyperlinks));
        }

        return shapedObjects;
    }
}
