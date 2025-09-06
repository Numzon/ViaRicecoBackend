using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using ViaRiceco.Common.Presentation.Abstractions.Headers;

namespace ViaRiceco.Common.Presentation.Abstractions.Collections;

public abstract class CollectionQueryParameters : BaseAcceptHeader, ICollectionQueryParameters
{
    [FromQuery(Name = "q")] public string? Search { get; set; }
    public string? Sort { get; init; }
    public string? Fields { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

[UsedImplicitly]
public sealed class ViaRicecoCollectionQueryParameters : CollectionQueryParameters;
