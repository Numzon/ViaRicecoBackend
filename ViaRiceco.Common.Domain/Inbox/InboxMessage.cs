namespace ViaRiceco.Common.Domain.Inbox;

public sealed class InboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; }
    public string Content { get; private set; }
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public string? Error { get; private set; }

    public static InboxMessage Create(Guid id, string type, string content, DateTime occurredOnUtc)
    {
        return new InboxMessage
        {
            Id = id,
            Type = type,
            Content = content,
            OccurredOnUtc = occurredOnUtc
        };
    }
    
    public void Update(DateTime processedOnUtc, Exception? error)
    {
        ProcessedOnUtc = processedOnUtc;
        Error = error?.ToString();
    }
}
