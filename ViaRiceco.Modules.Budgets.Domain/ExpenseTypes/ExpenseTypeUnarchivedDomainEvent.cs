using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;

public sealed class ExpenseTypeUnarchivedDomainEvent(
    string expenseTypeId,
    string name,
    DateTime unarchivedAtUtc) : DomainEvent
{
    public string ExpenseTypeId { get; init; } = expenseTypeId;
    public string Name { get; init; } = name;
    public DateTime UnarchivedAtUtc { get; init; } = unarchivedAtUtc;
}
