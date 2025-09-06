using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Application.Services.Sorting.Models;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;

namespace ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;

public class TaxTypeDtoSortMappingSource : ISortMappingSource
{
    public ISortMappingDefinition GetSortMappingDefinition()
    {
        return SortMappingDefinition.Create<TaxTypeDto, TaxType>([
            new SortMapping(nameof(TaxTypeDto.Id), nameof(TaxType.Id)),
            new SortMapping(nameof(TaxTypeDto.Name), nameof(TaxType.Name)),
        ]);
    }
}
