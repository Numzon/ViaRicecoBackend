using Microsoft.EntityFrameworkCore;
using ViaRiceco.Common.Domain.Outbox;
using ViaRiceco.Modules.Accounting.Domain.Outbox;
using ViaRiceco.Modules.Accounting.Infrastructure.Database;

namespace ViaRiceco.Modules.Accounting.Infrastructure.Outbox;

internal sealed class OutboxMessageConsumerRepository(AccountingDbContext context) : IOutboxMessageConsumerRepository
{
    public Task<bool> ExistsAsync(Guid outboxMessageId, string name, CancellationToken cancellationToken = default)
    {
        return context.OutboxMessageConsumers
            .AnyAsync(x => x.OutboxMessageId == outboxMessageId && x.Name == name, cancellationToken);
    }

    public void Insert(OutboxMessageConsumer consumer)
    {
        context.OutboxMessageConsumers.Add(consumer);
    }
}
