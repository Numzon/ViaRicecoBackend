using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

public sealed class ExpenseRemovedFromMonthlyBudgetDomainEvent(
    string monthlyBudgetId,
    string monthlyBudgetExpenseId,
    DateTime removedAtUtc) : DomainEvent
{
    public string MonthlyBudgetId { get; init; } = monthlyBudgetId;
    public string MonthlyBudgetExpenseId { get; init; } = monthlyBudgetExpenseId;
    public DateTime RemovedAtUtc { get; init; } = removedAtUtc;
}
