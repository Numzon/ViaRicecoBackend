using ViaRiceco.Common.Application.Data;
using ViaRiceco.Common.Application.Messaging;
using ViaRiceco.Common.Domain.Interfaces;
using ViaRiceco.Common.Domain.Outbox;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Domain.Outbox;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.Outbox;

internal sealed class IdempotentDomainEventHandler<TDomainEvent>(
    IUnitOfWork unitOfWork,
    IOutboxMessageConsumerRepository outboxMessageConsumerRepository,
    IDomainEventHandler<TDomainEvent> decorated)
    : DomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    public override async Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        bool exists =
            await outboxMessageConsumerRepository.ExistsAsync(domainEvent.Id, domainEvent.GetType().Name,
                cancellationToken);

        if (exists)
        {
            return;
        }

        await decorated.Handle(domainEvent, cancellationToken);
        
        var outboxMessageConsumer = OutboxMessageConsumer.Create(domainEvent.Id, decorated.GetType().Name);

        outboxMessageConsumerRepository.Insert(outboxMessageConsumer);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
