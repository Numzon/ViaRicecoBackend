using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

public sealed class TaxRemovedFromSettlementPeriodDomainEvent(string settlementPeriodId, string taxId, DateTime removedAtUtc) : DomainEvent
{
    public string SettlementPeriodId { get; init; } = settlementPeriodId;
    public string TaxId { get; init; } = taxId;
    public DateTime RemovedAtUtc { get; init; } = removedAtUtc;  
}
