using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace ViaRiceco.Common.Application.Abstractions.Collections;

public abstract class CollectionQueryParameters : ICollectionQueryParameters    
{
    [FromQuery(Name = "q")]
    public string? Search { get; set; }
    public string? Sort { get; init; }
    public string? Fields { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

[UsedImplicitly]
public sealed class ViaRicecoCollectionQueryParameters : CollectionQueryParameters;
