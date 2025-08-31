using ViaRiceco.Common.Application.Services.Sorting.Models;

namespace ViaRiceco.Common.Application.Services.Sorting;

public interface ISortMappingDefinition
{
    string SourceType { get; }
    string DestinationType { get; }
    SortMapping[] Mappings { get; }
}