using System.Dynamic;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;

namespace ViaRiceco.Common.Application.Services.DataShapers;

public interface IDataShapingService
{
    ExpandoObject ShapeData<T>(T entity, string? fields = null);
    ExpandoObject ShapeData<T>(T entity, Hyperlink[] hyperlinks);
    bool Validate<T>(string? fields);
}
