using FastEndpoints;
using JetBrains.Annotations;
using ViaRiceco.Common.Presentation.Abstractions.Headers;

namespace ViaRiceco.Common.Presentation.Abstractions.Collections;

public abstract class CollectionQueryParameters : BaseAcceptHeader, ICollectionQueryParameters
{
    [BindFrom("q")] 
    public string? Search { get; set; }
    public string? Sort { get; init; }
    public string? Fields { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

[UsedImplicitly]
public sealed class ViaRicecoCollectionQueryParameters : CollectionQueryParameters;
