using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Application.Services.Sorting.Models;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;

namespace ViaRiceco.Modules.Accounting.Application.TaxTypes.Models;

public static class TaxTypeMappings
{
    public static readonly SortMappingDefinition<TaxTypeDto, TaxType> SortMapping = new()
     {
         Mappings = [
             new SortMapping(nameof(TaxTypeDto.Id), nameof(TaxType.Id)),
             new SortMapping(nameof(TaxTypeDto.Name), nameof(TaxType.Name)),
         ]
     };
}
