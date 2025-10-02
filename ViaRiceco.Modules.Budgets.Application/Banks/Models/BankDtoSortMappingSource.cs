using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Application.Services.Sorting.Models;
using ViaRiceco.Modules.Budgets.Domain.Banks;

namespace ViaRiceco.Modules.Budgets.Application.Banks.Models;

public sealed class BankDtoSortMappingSource : ISortMappingSource
{    
    public ISortMappingDefinition GetSortMappingDefinition()
    {
        return SortMappingDefinition.Create<BankDto, Bank>([
            new SortMapping(nameof(BankDto.Id), nameof(Bank.Id)),
            new SortMapping(nameof(BankDto.Name), nameof(Bank.Name)),
        ]);
    }
}
