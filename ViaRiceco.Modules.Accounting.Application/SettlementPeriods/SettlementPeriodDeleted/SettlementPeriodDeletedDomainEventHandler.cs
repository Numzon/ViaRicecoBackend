using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Application.Messaging;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;
using ViaRiceco.Modules.Accounting.IntegrationEvents.SettlementPeriods;

namespace ViaRiceco.Modules.Accounting.Application.SettlementPeriods.SettlementPeriodDeleted;

public sealed class SettlementPeriodDeletedDomainEventHandler(
    IEventBus bus) : DomainEventHandler<SettlementPeriodDeletedDomainEvent>
{
    public override async Task Handle(SettlementPeriodDeletedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var integrationEvent = new SettlementPeriodDeletedIntegrationEvent(
            domainEvent.Id,
            domainEvent.OccurredOnUtc,
            domainEvent.SettlementPeriodId,
            domainEvent.Month,
            domainEvent.Year);

        await bus.PublishAsync(integrationEvent, cancellationToken);
    }
}
