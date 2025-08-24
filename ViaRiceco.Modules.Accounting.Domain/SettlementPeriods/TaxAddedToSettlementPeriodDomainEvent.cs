using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

public sealed class TaxAddedToSettlementPeriodDomainEvent(string settlementPeriodId, string taxId, decimal value, string taxTypeId, DateTime createdAtUtc) : DomainEvent
{
    public string SettlementPeriodId { get; init; } = settlementPeriodId;
    public string TaxId { get; init; } = taxId;
    public decimal Value { get; init; } = value;
    public string TaxTypeId { get; init; } = taxTypeId;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;  
}
