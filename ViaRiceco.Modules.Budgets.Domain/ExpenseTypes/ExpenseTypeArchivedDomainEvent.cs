using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;

public sealed class ExpenseTypeArchivedDomainEvent(
    string expenseTypeId,
    string name,
    DateTime archivedAtUtc) : DomainEvent
{
    public string ExpenseTypeId { get; init; } = expenseTypeId;
    public string Name { get; init; } = name;
    public DateTime ArchivedAtUtc { get; init; } = archivedAtUtc;
}
