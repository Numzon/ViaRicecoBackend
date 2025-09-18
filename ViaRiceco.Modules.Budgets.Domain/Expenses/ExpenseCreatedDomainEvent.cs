using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.Expenses;

public sealed class ExpenseCreatedDomainEvent(
    string expenseId, 
    string name, 
    string expenseTypeId, 
    DateTime createdAtUtc) : DomainEvent
{
    public string ExpenseId { get; init; } = expenseId;
    public string Name { get; init; } = name;
    public string ExpenseTypeId { get; init; } = expenseTypeId;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;
}
