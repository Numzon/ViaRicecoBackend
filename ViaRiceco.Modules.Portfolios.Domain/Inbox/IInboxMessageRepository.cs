using ViaRiceco.Common.Domain.Inbox;

namespace ViaRiceco.Modules.Portfolios.Domain.Inbox;

public interface IInboxMessageRepository
{
    Task<IReadOnlyList<InboxMessage>> GetInboxMessagesAsync(int batchSize,
        CancellationToken cancellationToken = default);
    
    void Insert(InboxMessage message);  
}
