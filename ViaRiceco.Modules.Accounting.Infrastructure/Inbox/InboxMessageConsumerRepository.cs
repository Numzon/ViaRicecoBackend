using Microsoft.EntityFrameworkCore;
using ViaRiceco.Common.Domain.Inbox;
using ViaRiceco.Modules.Accounting.Domain.Inbox;
using ViaRiceco.Modules.Accounting.Infrastructure.Database;

namespace ViaRiceco.Modules.Accounting.Infrastructure.Inbox;

internal sealed class InboxMessageConsumerRepository(AccountingDbContext context) : IInboxMessageConsumerRepository
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
