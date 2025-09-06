using System.Dynamic;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;

namespace ViaRiceco.Common.Application.Services.DataShapers;

public interface IDataShapingService
{
    ExpandoObject ShapeData<T>(T entity, string? fields = null, Hyperlink[]? hyperlinks = null);
    IReadOnlyCollection<ExpandoObject> ShapeCollectionData<T>(IReadOnlyCollection<T> entities, string? fields = null,
        Func<T, Hyperlink[]>? generateLinks = null);

    bool Validate<T>(string? fields);
}
