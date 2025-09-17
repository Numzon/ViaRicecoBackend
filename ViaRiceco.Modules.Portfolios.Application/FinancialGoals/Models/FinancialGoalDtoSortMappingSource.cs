using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Application.Services.Sorting.Models;
using ViaRiceco.Modules.Portfolios.Domain.FinancialGoals;

namespace ViaRiceco.Modules.Portfolios.Application.FinancialGoals.Models;

public class FinancialGoalDtoSortMappingSource : ISortMappingSource
{
    public ISortMappingDefinition GetSortMappingDefinition()
    {
        return SortMappingDefinition.Create<FinancialGoalDto, FinancialGoal>([
            new SortMapping(nameof(FinancialGoalDto.Id), nameof(FinancialGoal.Id)),
            new SortMapping(nameof(FinancialGoalDto.Name), nameof(FinancialGoal.Name)),
            new SortMapping(nameof(FinancialGoalDto.ParentId), nameof(FinancialGoal.ParentId)),
        ]);
    }
}
