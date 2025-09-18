using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.Expenses;

public sealed class ExpenseCreatedFromIntegrationEventDomainEvent(
    string expenseId,
    string name,
    string expenseTypeId,
    DateTime createdAtUtc) : DomainEvent
{
    public string ExpenseId { get; } = expenseId;
    public string Name { get; } = name;
    public string ExpenseTypeId { get; } = expenseTypeId;
    public DateTime CreatedAtUtc { get; } = createdAtUtc;
}
