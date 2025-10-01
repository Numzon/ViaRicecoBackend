using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

public sealed class ExpenseAddedToMonthlyBudgetDomainEvent(
    string monthlyBudgetId,
    string expenseId,
    string expenseName,
    DateTime addedAtUtc) : DomainEvent
{
    public string MonthlyBudgetId { get; init; } = monthlyBudgetId;
    public string ExpenseId { get; init; } = expenseId;
    public string ExpenseName { get; init; } = expenseName;
    public DateTime AddedAtUtc { get; init; } = addedAtUtc;
}
