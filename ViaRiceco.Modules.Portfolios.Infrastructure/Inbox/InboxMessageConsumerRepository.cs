using Microsoft.EntityFrameworkCore;
using ViaRiceco.Common.Domain.Inbox;
using ViaRiceco.Modules.Portfolios.Domain.Inbox;
using ViaRiceco.Modules.Portfolios.Infrastructure.Database;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.Inbox;

internal sealed class InboxMessageConsumerRepository(PortfoliosDbContext context) : IInboxMessageConsumerRepository
{
    public Task<bool> ExistsAsync(Guid outboxMessageId, string name, CancellationToken cancellationToken = default)
    {
        return context.OutboxMessageConsumers
            .AnyAsync(x => x.OutboxMessageId == outboxMessageId && x.Name == name, cancellationToken);
    }

    public void Insert(InboxMessageConsumer consumer)
    {
        context.InboxMessageConsumers.Add(consumer);
    }
}
