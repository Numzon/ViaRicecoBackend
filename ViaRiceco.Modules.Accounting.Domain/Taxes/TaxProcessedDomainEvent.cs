using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.Taxes;

public sealed class TaxProcessedDomainEvent(string taxId, string taxTypeId, string settlementPeriodId, DateTime createdAtUtc) : DomainEvent
{
    public string TaxId { get; init; } = taxId;
    public string TaxTypeId { get; init; } = taxTypeId;
    public string SettlementPeriodId { get; init; } = settlementPeriodId;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;  
}
