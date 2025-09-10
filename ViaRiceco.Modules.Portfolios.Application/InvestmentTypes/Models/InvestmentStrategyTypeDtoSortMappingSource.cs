using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Application.Services.Sorting.Models;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentTypes;

namespace ViaRiceco.Modules.Portfolios.Application.InvestmentTypes.Models;

public class InvestmentStrategyTypeDtoSortMappingSource : ISortMappingSource
{
    public ISortMappingDefinition GetSortMappingDefinition()
    {
        return SortMappingDefinition.Create<InvestmentStrategyTypeDto, InvestmentStrategyType>([
            new SortMapping(nameof(InvestmentStrategyTypeDto.Id), nameof(InvestmentStrategyType.Id)),
            new SortMapping(nameof(InvestmentStrategyTypeDto.Name), nameof(InvestmentStrategyType.Name)),
        ]);
    }
}
