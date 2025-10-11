using ViaRiceco.Common.Application.EventBus;

namespace ViaRiceco.Modules.Accounting.IntegrationEvents.SettlementPeriods;

public sealed class SettlementPeriodCreatedIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    string settlementPeriodId,
    int month,
    int year,
    decimal netAmount)
    : IntegrationEvent(id, occurredOnUtc)
{
    public string SettlementPeriodId { get; init; } = settlementPeriodId;
    public int Month { get; init; } = month;
    public int Year { get; init; } = year;
    public decimal NetAmount { get; init; } = netAmount;
}
