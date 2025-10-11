using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Application.Messaging;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;
using ViaRiceco.Modules.Accounting.IntegrationEvents.SettlementPeriods;

namespace ViaRiceco.Modules.Accounting.Application.SettlementPeriods.SettlementPeriodNetAmountRecalculated;

public sealed class SettlementPeriodNetAmountRecalculatedDomainEventHandler(
    IEventBus bus) : DomainEventHandler<SettlementPeriodNetAmountRecalculatedDomainEvent>
{
    public override async Task Handle(SettlementPeriodNetAmountRecalculatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var integrationEvent = new SettlementPeriodNetAmountRecalculatedIntegrationEvent(
            domainEvent.Id,
            domainEvent.OccurredOnUtc,
            domainEvent.SettlementPeriodId,
            domainEvent.TotalIncome,
            domainEvent.TotalTaxes,
            domainEvent.NetAmount);

        await bus.PublishAsync(integrationEvent, cancellationToken);
    }
}
