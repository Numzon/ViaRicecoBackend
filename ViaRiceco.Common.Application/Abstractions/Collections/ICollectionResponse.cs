using ViaRiceco.Common.Application.Services.Hyperlinks.Models;

namespace ViaRiceco.Common.Application.Abstractions.Collections;

public interface ICollectionResponse<T>
{
    IEnumerable<T> Items { get; init; }
    int Page { get; init; }
    int PageSize { get; init; }
    int TotalCount { get; init; }
    IReadOnlyCollection<Hyperlink> Links { get; set; }
}
