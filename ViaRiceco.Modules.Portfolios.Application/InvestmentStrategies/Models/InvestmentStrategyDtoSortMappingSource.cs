using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Application.Services.Sorting.Models;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

namespace ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.Models;

public class InvestmentStrategyDtoSortMappingSource : ISortMappingSource
{
    public ISortMappingDefinition GetSortMappingDefinition()
    {
        return SortMappingDefinition.Create<InvestmentStrategyDto, InvestmentStrategy>([
            new SortMapping(nameof(InvestmentStrategyDto.Id), nameof(InvestmentStrategy.Id)),
            new SortMapping(nameof(InvestmentStrategyDto.FinancialGoalId), nameof(InvestmentStrategy.FinancialGoalId)),
            new SortMapping(nameof(InvestmentStrategyDto.InvestmentStrategyTypeId), nameof(InvestmentStrategy.InvestmentStrategyTypeId)),
            new SortMapping(nameof(InvestmentStrategyDto.UninvestedAmount), nameof(InvestmentStrategy.UninvestedAmount)),
            new SortMapping(nameof(InvestmentStrategyDto.TotalInvestedAmount), nameof(InvestmentStrategy.TotalInvestedAmount)),
            new SortMapping(nameof(InvestmentStrategyDto.TotalCurrentAmount), nameof(InvestmentStrategy.TotalCurrentAmount)),
            new SortMapping(nameof(InvestmentStrategyDto.TotalAmount), nameof(InvestmentStrategy.TotalAmount)),
        ]);
    }
}
