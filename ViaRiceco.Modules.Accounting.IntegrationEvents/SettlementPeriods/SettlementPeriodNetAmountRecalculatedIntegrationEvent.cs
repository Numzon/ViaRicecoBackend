using ViaRiceco.Common.Application.EventBus;

namespace ViaRiceco.Modules.Accounting.IntegrationEvents.SettlementPeriods;

public sealed class SettlementPeriodNetAmountRecalculatedIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    string settlementPeriodId,
    decimal totalIncome,
    decimal totalTaxes,
    decimal netAmount)
    : IntegrationEvent(id, occurredOnUtc)
{
    public string SettlementPeriodId { get; init; } = settlementPeriodId;
    public decimal TotalIncome { get; init; } = totalIncome;
    public decimal TotalTaxes { get; init; } = totalTaxes;
    public decimal NetAmount { get; init; } = netAmount;
}
