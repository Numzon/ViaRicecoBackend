namespace ViaRiceco.Common.Domain.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }

    public string Type { get; private set; }

    public string Content { get; private set; }

    public DateTime OccurredOnUtc { get; private set; }

    public DateTime? ProcessedOnUtc { get; private set; }

    public string? Error { get; private set; }
    

    public void Update(DateTime processedOnUtc, Exception? error)
    {
        ProcessedOnUtc = processedOnUtc;
        Error = error?.ToString();
    }

    public static OutboxMessage Create(Guid id, string type, string content, DateTime occurredOnUtc)
    {
        return new OutboxMessage
        {
            Id = id,
            Type = type,
            Content = content,
            OccurredOnUtc = occurredOnUtc
        };
    }
}
