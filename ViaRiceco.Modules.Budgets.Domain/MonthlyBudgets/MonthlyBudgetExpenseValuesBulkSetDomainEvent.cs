using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

public sealed class MonthlyBudgetExpenseValuesBulkSetDomainEvent(
    string monthlyBudgetId,
    IReadOnlyCollection<ExpenseValueUpdate> expenseValueUpdates,
    DateTime updatedAtUtc) : DomainEvent(Guid.NewGuid(), updatedAtUtc)
{
    public string MonthlyBudgetId { get; init; } = monthlyBudgetId;
    public IReadOnlyCollection<ExpenseValueUpdate> ExpenseValueUpdates { get; init; } = expenseValueUpdates;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;
}
