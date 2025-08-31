using ViaRiceco.Common.Application.Services.Sorting.Models;

namespace ViaRiceco.Common.Application.Services.Sorting;

public sealed class SortMappingDefinition : ISortMappingDefinition
{
    public required string SourceType { get; init; }
    public required string DestinationType { get; init; }
    public required SortMapping[] Mappings { get; init; }
    
    public static SortMappingDefinition Create<TSource, TDestination>(SortMapping[] mappings)
    {
        return new SortMappingDefinition
        {
            SourceType = typeof(TSource).Name,
            DestinationType = typeof(TDestination).Name,
            Mappings = mappings
        };
    }
}
