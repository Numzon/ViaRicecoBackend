using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Application.Services.Sorting.Models;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

namespace ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models;

public class SettlementPeriodDtoSortMappingSource : ISortMappingSource
{
    public ISortMappingDefinition GetSortMappingDefinition()
    {
        return SortMappingDefinition.Create<SettlementPeriodDto, SettlementPeriod>([
            new SortMapping(nameof(SettlementPeriodDto.Id), nameof(SettlementPeriod.Id)),
            new SortMapping(nameof(SettlementPeriodDto.Month), nameof(SettlementPeriod.Month)),
            new SortMapping(nameof(SettlementPeriodDto.Year), nameof(SettlementPeriod.Year)),
            new SortMapping(nameof(SettlementPeriodDto.TotalIncome), nameof(SettlementPeriod.TotalIncome)),
            new SortMapping(nameof(SettlementPeriodDto.TotalTaxes), nameof(SettlementPeriod.TotalTaxes)),
            new SortMapping(nameof(SettlementPeriodDto.NetAmount), nameof(SettlementPeriod.NetAmount)),
            new SortMapping(nameof(SettlementPeriodDto.CreatedAtUtc), nameof(SettlementPeriod.CreatedAtUtc)),
            new SortMapping(nameof(SettlementPeriodDto.UpdatedAtUtc), nameof(SettlementPeriod.UpdatedAtUtc))
        ]);
    }
}
