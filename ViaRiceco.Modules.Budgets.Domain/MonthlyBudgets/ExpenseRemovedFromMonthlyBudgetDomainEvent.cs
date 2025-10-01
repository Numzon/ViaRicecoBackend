using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

public sealed class ExpenseRemovedFromMonthlyBudgetDomainEvent(
    string monthlyBudgetId,
    string expenseId,
    DateTime removedAtUtc) : DomainEvent
{
    public string MonthlyBudgetId { get; init; } = monthlyBudgetId;
    public string ExpenseId { get; init; } = expenseId;
    public DateTime RemovedAtUtc { get; init; } = removedAtUtc;
}
