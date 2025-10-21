using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.Expenses;

public sealed class ExpenseArchivedDomainEvent(
    string expenseId, 
    string name, 
    string expenseTypeId, 
    DateTime archivedAtUtc) : DomainEvent
{
    public string ExpenseId { get; init; } = expenseId;
    public string Name { get; init; } = name;
    public string ExpenseTypeId { get; init; } = expenseTypeId;
    public DateTime ArchivedAtUtc { get; init; } = archivedAtUtc;
}
