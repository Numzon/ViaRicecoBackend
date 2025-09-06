using Microsoft.AspNetCore.Http;

namespace ViaRiceco.Common.Application.Services.ContentType;

public interface IContentTypeService
{
    bool ShouldIncludeHateoasLinks();
    string GetResponseContentType(string? version = null);
}
