using ViaRiceco.Common.Domain.Outbox;

namespace ViaRiceco.Modules.Budgets.Domain.Outbox;

public interface IOutboxMessageConsumerRepository
{
    Task<bool> ExistsAsync(Guid outboxMessageId, string name, CancellationToken cancellationToken = default);
    void Insert(OutboxMessageConsumer consumer);
}

