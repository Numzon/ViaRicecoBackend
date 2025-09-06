using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Common.Application.Services.Sorting;

public interface ISortingService
{
    string GenerateOrderByClause<TSource, TDestination>(string? sort, string defaultOrderBy = "Id");
    bool ValidateSortParameters<TSource, TDestination>(string? sort);
}
