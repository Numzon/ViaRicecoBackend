using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Application.Services.Sorting.Models;
using ViaRiceco.Modules.Budgets.Domain.Expenses;

namespace ViaRiceco.Modules.Budgets.Application.Expenses.Models;

public class ExpenseDtoSortMappingSource : ISortMappingSource
{
    public ISortMappingDefinition GetSortMappingDefinition()
    {
        return SortMappingDefinition.Create<ExpenseDto, Expense>([
            new SortMapping(nameof(ExpenseDto.Id), nameof(Expense.Id)),
            new SortMapping(nameof(ExpenseDto.Name), nameof(Expense.Name)),
            new SortMapping(nameof(ExpenseDto.ExpenseTypeId), nameof(Expense.ExpenseTypeId)),
        ]);
    }
}
