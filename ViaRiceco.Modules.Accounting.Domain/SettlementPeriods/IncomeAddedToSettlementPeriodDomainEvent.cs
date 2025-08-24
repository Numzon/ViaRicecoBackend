using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

public sealed class IncomeAddedToSettlementPeriodDomainEvent(string settlementPeriodId, string incomeId, decimal value, DateTime createdAtUtc) : DomainEvent
{
    public string SettlementPeriodId { get; init; } = settlementPeriodId;
    public string IncomeId { get; init; } = incomeId;
    public decimal Value { get; init; } = value;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;  
}
