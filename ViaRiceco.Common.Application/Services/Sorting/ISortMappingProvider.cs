using ViaRiceco.Common.Application.Services.Sorting.Models;

namespace ViaRiceco.Common.Application.Services.Sorting;

public interface ISortMappingProvider
{
    SortMapping[] GetMappings<TSource, TDestination>();
}
