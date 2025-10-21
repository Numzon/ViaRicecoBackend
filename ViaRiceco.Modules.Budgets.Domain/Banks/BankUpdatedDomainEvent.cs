using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.Banks;

public sealed class BankUpdatedDomainEvent(
    string bankId,
    string name,
    DateTime updatedAtUtc) : DomainEvent(Guid.NewGuid(), updatedAtUtc)
{
    public string BankId { get; init; } = bankId;
    public string Name { get; init; } = name;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;
}
