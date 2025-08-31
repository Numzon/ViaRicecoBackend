using System.Dynamic;
using System.Text.Json.Serialization;
using ViaRiceco.Common.Application.Services.Hyperlinks.Models;

namespace ViaRiceco.Common.Application.Abstractions.Collections;

public abstract class CollectionResponse<T> : ICollectionResponse<T>
{
    public IEnumerable<T> Items { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    
    [JsonPropertyName("_links")]
    public IReadOnlyCollection<Hyperlink> Links { get; set; }
    
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

public sealed class ViaRicecoCollectionResponse : CollectionResponse<ExpandoObject>;
