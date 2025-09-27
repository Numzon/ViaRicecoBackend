namespace ViaRiceco.Common.Domain.Inbox;

public class InboxMessageConsumer
{
    public Guid InboxMessageId { get; private set; }
    public string Name { get; private set; }

    public static InboxMessageConsumer Create(Guid inboxMessageId, string name)
    {
        return new InboxMessageConsumer
        {
            Name = name,
            InboxMessageId = inboxMessageId
        };
    }
}
