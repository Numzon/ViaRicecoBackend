namespace ViaRiceco.Common.Domain.Interfaces;

public interface IDomainEvent
{
    Guid Id { get; }

    DateTime OccurredOnUtc { get; }
}
