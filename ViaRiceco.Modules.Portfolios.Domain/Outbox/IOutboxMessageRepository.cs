using ViaRiceco.Common.Domain.Outbox;

namespace ViaRiceco.Modules.Portfolios.Domain.Outbox;

public interface IOutboxMessageRepository
{
    Task<IReadOnlyList<OutboxMessage>> GetOutboxMessagesAsync(int batchSize,
        CancellationToken cancellationToken = default);
}
