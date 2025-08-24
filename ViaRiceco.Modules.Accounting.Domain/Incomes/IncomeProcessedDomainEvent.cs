using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.Incomes;

public sealed class IncomeProcessedDomainEvent(string incomeId, string settlementPeriodId, DateTime createdAtUtc) : DomainEvent
{
    public string IncomeId { get; init; } = incomeId;
    public string SettlementPeriodId { get; init; } = settlementPeriodId;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;  
}
