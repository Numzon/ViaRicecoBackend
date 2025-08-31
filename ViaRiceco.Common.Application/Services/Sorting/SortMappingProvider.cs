using ViaRiceco.Common.Application.Services.Sorting.Models;

namespace ViaRiceco.Common.Application.Services.Sorting;

public sealed class SortMappingProvider(IEnumerable<ISortMappingDefinition> sortMappingDefinitions) : ISortMappingProvider
{
    public SortMapping[] GetMappings<TSource, TDestination>()
    {
        string sourceType = typeof(TSource).Name;
        string destinationType = typeof(TDestination).Name;
        
        ISortMappingDefinition? definition = sortMappingDefinitions
            .FirstOrDefault(x => x.SourceType == sourceType && x.DestinationType == destinationType);

        if (definition is null)
        {
            throw new InvalidOperationException($"No sort mapping definition found for {sourceType} to {destinationType}");
        }
        
        return definition.Mappings;
    }
}
