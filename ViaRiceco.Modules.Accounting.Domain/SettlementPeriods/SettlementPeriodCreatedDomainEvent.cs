using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

public sealed class SettlementPeriodCreatedDomainEvent(string settlementPeriodId, DateTime createdAtUtc) : DomainEvent
{
    public string SettlementPeriodId { get; init; } = settlementPeriodId;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;  
}
