using ViaRiceco.Common.Domain.Inbox;

namespace ViaRiceco.Modules.Budgets.Domain.Inbox;

public interface IInboxMessageConsumerRepository
{
    Task<bool> ExistsAsync(Guid outboxMessageId, string name, CancellationToken cancellationToken = default);
    void Insert(InboxMessageConsumer consumer);
}

