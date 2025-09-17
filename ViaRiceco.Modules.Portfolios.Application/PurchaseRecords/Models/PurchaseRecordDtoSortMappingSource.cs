using ViaRiceco.Common.Application.Services.Sorting;
using ViaRiceco.Common.Application.Services.Sorting.Models;
using ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;

namespace ViaRiceco.Modules.Portfolios.Application.PurchaseRecords.Models;

public class PurchaseRecordDtoSortMappingSource : ISortMappingSource
{
    public ISortMappingDefinition GetSortMappingDefinition()
    {
        return SortMappingDefinition.Create<PurchaseRecordDto, PurchaseRecord>([
            new SortMapping(nameof(PurchaseRecordDto.Id), nameof(PurchaseRecord.Id)),
            new SortMapping(nameof(PurchaseRecordDto.PurchaseDate), nameof(PurchaseRecord.PurchaseDate)),
            new SortMapping(nameof(PurchaseRecordDto.Amount), nameof(PurchaseRecord.Amount)),
            new SortMapping(nameof(PurchaseRecordDto.PricePerUnit), nameof(PurchaseRecord.PricePerUnit)),
            new SortMapping(nameof(PurchaseRecordDto.TotalPrice), nameof(PurchaseRecord.TotalPrice)),
            new SortMapping(nameof(PurchaseRecordDto.CurrencyId), nameof(PurchaseRecord.CurrencyId)),
            new SortMapping(nameof(PurchaseRecordDto.InvestmentId), nameof(PurchaseRecord.InvestmentId)),
        ]);
    }
}
