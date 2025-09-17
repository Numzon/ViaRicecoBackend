namespace ViaRiceco.Common.Domain.Outbox;

public sealed class OutboxMessageConsumer
{
    public Guid OutboxMessageId { get; private set; }
    public string Name { get; private set; }
    

    public static OutboxMessageConsumer Create(Guid outboxMessageId, string name)
    {
        return new OutboxMessageConsumer
        {
            Name = name,
            OutboxMessageId = outboxMessageId
        };
    }
}
