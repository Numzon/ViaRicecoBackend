using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.MonthlyBudgetExpenses;

public sealed class MonthlyBudgetExpenseValueSetDomainEvent(
    string monthlyBudgetExpenseId,
    string monthlyBudgetId,
    decimal? value,
    DateTime updatedAtUtc) : DomainEvent
{
    public string MonthlyBudgetExpenseId { get; init; } = monthlyBudgetExpenseId;
    public string MonthlyBudgetId { get; init; } = monthlyBudgetId;
    public decimal? Value { get; init; } = value;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;
}
