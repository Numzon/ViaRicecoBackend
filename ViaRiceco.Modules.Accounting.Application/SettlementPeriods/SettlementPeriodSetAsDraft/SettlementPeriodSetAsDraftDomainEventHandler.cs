using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Application.Messaging;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;
using ViaRiceco.Modules.Accounting.IntegrationEvents.SettlementPeriods;

namespace ViaRiceco.Modules.Accounting.Application.SettlementPeriods.SettlementPeriodSetAsDraft;

public sealed class SettlementPeriodSetAsDraftDomainEventHandler(
    IEventBus bus) : DomainEventHandler<SettlementPeriodSetAsDraftDomainEvent>
{
    public override async Task Handle(SettlementPeriodSetAsDraftDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var integrationEvent = new SettlementPeriodSetAsDraftIntegrationEvent(
            domainEvent.Id,
            domainEvent.OccurredOnUtc,
            domainEvent.SettlementPeriodId);

        await bus.PublishAsync(integrationEvent, cancellationToken);
    }
}
