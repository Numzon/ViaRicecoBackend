namespace ViaRiceco.Common.Presentation.Abstractions.Collections;

public interface ICollectionQueryParameters
{
    string? Search { get; set; }
    string? Sort { get; init; }
    int Page { get; init; }
    int PageSize { get; init; }
}
