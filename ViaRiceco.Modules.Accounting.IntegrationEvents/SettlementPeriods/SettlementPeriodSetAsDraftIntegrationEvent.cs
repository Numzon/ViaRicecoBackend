using ViaRiceco.Common.Application.EventBus;

namespace ViaRiceco.Modules.Accounting.IntegrationEvents.SettlementPeriods;

public sealed class SettlementPeriodSetAsDraftIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    string settlementPeriodId)
    : IntegrationEvent(id, occurredOnUtc)
{
    public string SettlementPeriodId { get; init; } = settlementPeriodId;
}
