using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Application.Messaging;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;
using ViaRiceco.Modules.Accounting.IntegrationEvents.SettlementPeriods;

namespace ViaRiceco.Modules.Accounting.Application.SettlementPeriods.SettlementPeriodCreated;

public sealed class SettlementPeriodCreatedDomainEventHandler(
    IEventBus bus,
    ISettlementPeriodRepository settlementPeriodRepository) : DomainEventHandler<SettlementPeriodCreatedDomainEvent>
{
    public override async Task Handle(SettlementPeriodCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        SettlementPeriod? settlementPeriod =
            await settlementPeriodRepository.GetAsync(domainEvent.SettlementPeriodId, cancellationToken);

        ArgumentNullException.ThrowIfNull(settlementPeriod);

        var integrationEvent = new SettlementPeriodCreatedIntegrationEvent(
            domainEvent.Id,
            domainEvent.OccurredOnUtc,
            settlementPeriod.Id,
            settlementPeriod.Month,
            settlementPeriod.Year);

        await bus.PublishAsync(integrationEvent, cancellationToken);
    }
}
