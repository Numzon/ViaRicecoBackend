using Microsoft.EntityFrameworkCore;
using ViaRiceco.Common.Domain.Outbox;
using ViaRiceco.Modules.Portfolios.Domain.Outbox;
using ViaRiceco.Modules.Portfolios.Infrastructure.Database;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.Outbox;

internal sealed class OutboxMessageConsumerRepository(PortfoliosDbContext context) : IOutboxMessageConsumerRepository
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
