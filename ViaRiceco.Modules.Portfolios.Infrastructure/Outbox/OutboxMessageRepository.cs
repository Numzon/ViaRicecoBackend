using Microsoft.EntityFrameworkCore;
using ViaRiceco.Common.Domain.Outbox;
using ViaRiceco.Modules.Portfolios.Domain.Outbox;
using ViaRiceco.Modules.Portfolios.Infrastructure.Database;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.Outbox;

internal sealed class OutboxMessageRepository(PortfoliosDbContext context) : IOutboxMessageRepository 
{
    public async Task<IReadOnlyList<OutboxMessage>> GetOutboxMessagesAsync(int batchSize,
        CancellationToken cancellationToken = default)
    {
        if (context.Database.CurrentTransaction == null)
        {
            throw new InvalidOperationException("FOR UPDATE requires an active transaction");
        }
        
        return await context.OutboxMessages
            .FromSqlRaw(@"
                SELECT * FROM portfolios.outbox_messages 
                WHERE processed_on_utc IS NULL 
                ORDER BY occurred_on_utc 
                LIMIT {0} 
                FOR UPDATE", batchSize)
            .ToListAsync(cancellationToken);
    }
}
