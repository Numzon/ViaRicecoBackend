using Microsoft.EntityFrameworkCore;
using ViaRiceco.Common.Domain.Outbox;
using ViaRiceco.Modules.Budgets.Domain.Outbox;
using ViaRiceco.Modules.Budgets.Infrastructure.Database;

namespace ViaRiceco.Modules.Budgets.Infrastructure.Outbox;

internal sealed class OutboxMessageConsumerRepository(BudgetsDbContext context) : IOutboxMessageConsumerRepository
{
    public async Task<bool> ExistsAsync(Guid outboxMessageId, string name, CancellationToken cancellationToken = default)
    {
        return await context.OutboxMessageConsumers.AnyAsync(
            omc => omc.OutboxMessageId == outboxMessageId && omc.Name == name,
            cancellationToken);
    }

    public void Insert(OutboxMessageConsumer consumer) => context.OutboxMessageConsumers.Add(consumer);
}
