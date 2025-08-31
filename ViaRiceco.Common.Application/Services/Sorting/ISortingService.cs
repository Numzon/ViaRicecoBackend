using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Common.Application.Services.Sorting;

/// <summary>
/// Service for handling sorting operations with validation and mapping
/// </summary>
public interface ISortingService
{
    /// <summary>
    /// Generates OrderBy clause for the specified types (assumes sort parameters are already validated)
    /// </summary>
    /// <typeparam name="TSource">Source DTO type</typeparam>
    /// <typeparam name="TDestination">Destination entity type</typeparam>
    /// <param name="sort">Sort parameter string (e.g., "name asc, createdAt desc")</param>
    /// <param name="defaultOrderBy">Default ordering when sort is null or empty</param>
    /// <returns>OrderBy clause string for use in queries</returns>
    string GenerateOrderByClause<TSource, TDestination>(string? sort, string defaultOrderBy = "Id");

    /// <summary>
    /// Validates if the sort parameters are valid for the specified types
    /// </summary>
    /// <typeparam name="TSource">Source DTO type</typeparam>
    /// <typeparam name="TDestination">Destination entity type</typeparam>
    /// <param name="sort">Sort parameter string to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    bool ValidateSortParameters<TSource, TDestination>(string? sort);
}
