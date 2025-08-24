using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

public sealed class SettlementPeriodNetAmountRecalculatedDomainEvent(string settlementPeriodId, decimal totalIncome, decimal totalTaxes, decimal netAmount, DateTime recalculatedAtUtc) : DomainEvent
{
    public string SettlementPeriodId { get; init; } = settlementPeriodId;
    public decimal TotalIncome { get; init; } = totalIncome;
    public decimal TotalTaxes { get; init; } = totalTaxes;
    public decimal NetAmount { get; init; } = netAmount;
    public DateTime RecalculatedAtUtc { get; init; } = recalculatedAtUtc;  
}
