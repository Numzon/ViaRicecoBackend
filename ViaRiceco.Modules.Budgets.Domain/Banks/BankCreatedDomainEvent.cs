using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.Banks;

public sealed class BankCreatedDomainEvent(
    string bankId,
    string name,
    DateTime createdAtUtc) : DomainEvent(Guid.NewGuid(), createdAtUtc)
{
    public string BankId { get; init; } = bankId;
    public string Name { get; init; } = name;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;
}
