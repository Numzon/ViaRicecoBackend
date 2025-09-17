using Microsoft.EntityFrameworkCore;
using ViaRiceco.Common.Domain.Inbox;
using ViaRiceco.Modules.Budgets.Domain.Inbox;
using ViaRiceco.Modules.Budgets.Infrastructure.Database;

namespace ViaRiceco.Modules.Budgets.Infrastructure.Inbox;

internal sealed class InboxMessageConsumerRepository(BudgetsDbContext context) : IInboxMessageConsumerRepository
{
    public async Task<bool> ExistsAsync(Guid outboxMessageId, string name, CancellationToken cancellationToken = default)
    {
        return await context.InboxMessageConsumers.AnyAsync(
            omc => omc.InboxMessageId == outboxMessageId && omc.Name == name,
            cancellationToken);
    }

    public void Insert(InboxMessageConsumer consumer) => context.InboxMessageConsumers.Add(consumer);
}
