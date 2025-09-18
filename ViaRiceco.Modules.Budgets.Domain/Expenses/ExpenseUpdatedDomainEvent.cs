using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.Expenses;

public sealed class ExpenseUpdatedDomainEvent(
    string expenseId, 
    string name, 
    string expenseTypeId, 
    DateTime updatedAtUtc) : DomainEvent
{
    public string ExpenseId { get; init; } = expenseId;
    public string Name { get; init; } = name;
    public string ExpenseTypeId { get; init; } = expenseTypeId;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;
}

