using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

public sealed class IncomeRemovedFromSettlementPeriodDomainEvent(string settlementPeriodId, string incomeId, DateTime removedAtUtc) : DomainEvent
{
    public string SettlementPeriodId { get; init; } = settlementPeriodId;
    public string IncomeId { get; init; } = incomeId;
    public DateTime RemovedAtUtc { get; init; } = removedAtUtc;  
}
