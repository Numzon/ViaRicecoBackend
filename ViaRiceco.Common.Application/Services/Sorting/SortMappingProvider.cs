using ViaRiceco.Common.Application.Services.Sorting.Models;

namespace ViaRiceco.Common.Application.Services.Sorting;

public sealed class SortMappingProvider(IEnumerable<ISortMappingDefinition> sortMappingDefinitions) : ISortMappingProvider
{
    public SortMapping[] GetMappings<TSource, TDestination>()
    {
        SortMappingDefinition<TSource, TDestination>? definition = sortMappingDefinitions
            .OfType<SortMappingDefinition<TSource, TDestination>>()
            .FirstOrDefault();

        if (definition is null)
        {
            throw new InvalidOperationException($"No sort mapping definition found for {typeof(TSource).Name} to {typeof(TDestination).Name}");
        }
        return definition.Mappings;
    }
}
