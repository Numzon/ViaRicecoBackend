using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Application.Services.Sorting.Models;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;

namespace ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models;

public class ExpenseTypeDtoSortMappingSource : ISortMappingSource
{
    public ISortMappingDefinition GetSortMappingDefinition()
    {
        return SortMappingDefinition.Create<ExpenseTypeDto, ExpenseType>([
            new SortMapping(nameof(ExpenseTypeDto.Id), nameof(ExpenseType.Id)),
            new SortMapping(nameof(ExpenseTypeDto.Name), nameof(ExpenseType.Name)),
            new SortMapping(nameof(ExpenseTypeDto.IsSystemDefined), nameof(ExpenseType.IsSystemDefined)),
        ]);
    }
}
