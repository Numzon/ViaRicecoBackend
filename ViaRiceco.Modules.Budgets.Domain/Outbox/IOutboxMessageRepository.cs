using ViaRiceco.Common.Domain.Outbox;

namespace ViaRiceco.Modules.Budgets.Domain.Outbox;

public interface IOutboxMessageRepository
{
    Task<IReadOnlyList<OutboxMessage>> GetOutboxMessagesAsync(int batchSize,
        CancellationToken cancellationToken = default);
}

