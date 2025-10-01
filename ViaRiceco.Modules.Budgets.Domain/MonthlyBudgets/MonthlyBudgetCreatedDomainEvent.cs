using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

public sealed class MonthlyBudgetCreatedDomainEvent(
    string monthlyBudgetId,
    string settlementPeriodId,
    int month,
    int year,
    int expensesCount,
    DateTime createdAtUtc) : DomainEvent
{
    public string MonthlyBudgetId { get; init; } = monthlyBudgetId;
    public string SettlementPeriodId { get; init; } = settlementPeriodId;
    public int Month { get; init; } = month;
    public int Year { get; init; } = year;
    public int ExpensesCount { get; init; } = expensesCount;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;
}
