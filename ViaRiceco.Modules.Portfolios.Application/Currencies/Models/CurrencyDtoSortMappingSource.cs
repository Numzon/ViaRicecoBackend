using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Application.Services.Sorting.Models;
using ViaRiceco.Modules.Portfolios.Domain.Currencies;

namespace ViaRiceco.Modules.Portfolios.Application.Currencies.Models;

public class CurrencyDtoSortMappingSource : ISortMappingSource
{
    public ISortMappingDefinition GetSortMappingDefinition()
    {
        return SortMappingDefinition.Create<CurrencyDto, Currency>([
            new SortMapping(nameof(CurrencyDto.Id), nameof(Currency.Id)),
            new SortMapping(nameof(CurrencyDto.Name), nameof(Currency.Name)),
            new SortMapping(nameof(CurrencyDto.Code), nameof(Currency.Code)),
        ]);
    }
}
