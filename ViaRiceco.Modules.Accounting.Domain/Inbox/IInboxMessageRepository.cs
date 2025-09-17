using ViaRiceco.Common.Domain.Inbox;

namespace ViaRiceco.Modules.Accounting.Domain.Inbox;

public interface IInboxMessageRepository
{
    Task<IReadOnlyList<InboxMessage>> GetInboxMessagesAsync(int batchSize,
        CancellationToken cancellationToken = default);
}
