using FastEndpoints;
using Microsoft.Net.Http.Headers;
using ViaRiceco.Common.Presentation.Enumerations;

namespace ViaRiceco.Common.Presentation.Abstractions.Headers;

public abstract class BaseAcceptHeader
{
    [FromHeader] 
    public string? Accept { get; init; }

    public bool IncludeLinks => MediaTypeHeaderValue.TryParse(Accept, out MediaTypeHeaderValue? mediaType) &&
                                mediaType.SubTypeWithoutSuffix.HasValue &&
                                mediaType.SubTypeWithoutSuffix.Value.Contains(CustomMediaTypeNames.HateoasSubType);
}
