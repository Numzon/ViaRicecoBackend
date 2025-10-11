using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

public sealed class SettlementPeriodFinalizedDomainEvent(string settlementPeriodId, DateTime finalizedAtUtc) : DomainEvent
{
    public string SettlementPeriodId { get; init; } = settlementPeriodId;
    public DateTime FinalizedAtUtc { get; init; } = finalizedAtUtc;
}
