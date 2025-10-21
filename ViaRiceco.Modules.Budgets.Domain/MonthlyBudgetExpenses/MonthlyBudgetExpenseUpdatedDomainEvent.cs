using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.MonthlyBudgetExpenses;

public sealed class MonthlyBudgetExpenseUpdatedDomainEvent(
    string monthlyBudgetExpenseId,
    string monthlyBudgetId,
    string expenseId,
    string expenseName,
    DateTime updatedAtUtc) : DomainEvent(Guid.NewGuid(), updatedAtUtc)
{
    public string MonthlyBudgetExpenseId { get; init; } = monthlyBudgetExpenseId;
    public string MonthlyBudgetId { get; init; } = monthlyBudgetId;
    public string ExpenseId { get; init; } = expenseId;
    public string ExpenseName { get; init; } = expenseName;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;
}
