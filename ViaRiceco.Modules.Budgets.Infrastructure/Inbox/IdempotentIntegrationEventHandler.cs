using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Domain.Inbox;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Domain.Inbox;

namespace ViaRiceco.Modules.Budgets.Infrastructure.Inbox;

internal sealed class IdempotentIntegrationEventHandler<TIntegrationEvent>(
    IUnitOfWork unitOfWork,
    IInboxMessageConsumerRepository inboxMessageConsumerRepository,
    IIntegrationEventHandler<TIntegrationEvent> decorated)
    : IntegrationEventHandler<TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
    public override async Task Handle(
        TIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        bool exists =
            await inboxMessageConsumerRepository.ExistsAsync(integrationEvent.Id, integrationEvent.GetType().Name,
                cancellationToken);
        
        if (exists)
        {
            return;
        }
        
        await decorated.Handle(integrationEvent, cancellationToken);

        var outboxMessageConsumer = InboxMessageConsumer.Create(integrationEvent.Id, decorated.GetType().Name);

        inboxMessageConsumerRepository.Insert(outboxMessageConsumer);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

