using Microsoft.EntityFrameworkCore;
using ViaRiceco.Common.Domain.Inbox;
using ViaRiceco.Modules.Portfolios.Domain.Inbox;
using ViaRiceco.Modules.Portfolios.Infrastructure.Database;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.Inbox;

internal sealed class InboxMessageRepository(PortfoliosDbContext context) : IInboxMessageRepository
{
    public async Task<IReadOnlyList<InboxMessage>> GetInboxMessagesAsync(int batchSize,
        CancellationToken cancellationToken = default)
    {
        if (context.Database.CurrentTransaction == null)
        {
            throw new InvalidOperationException("FOR UPDATE requires an active transaction");
        }

        return await context.InboxMessages
            .FromSqlRaw(@"
                SELECT * FROM portfolios.inbox_messages 
                WHERE processed_on_utc IS NULL 
                ORDER BY occurred_on_utc 
                LIMIT {0} 
                FOR UPDATE", batchSize)
            .ToListAsync(cancellationToken);
    }

    public void Insert(InboxMessage message)
    {
        context.InboxMessages.Add(message); 
    }
}
