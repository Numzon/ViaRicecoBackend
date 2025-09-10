namespace ViaRiceco.Common.Domain.Models;

public sealed class BaseQueryParameters
{
    public string? Search { get; init; }
    public string OrderBy { get; init; } = "CreatedAtUtc";
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
