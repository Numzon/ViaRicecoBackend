using ViaRiceco.Common.Application.EventBus;

namespace ViaRiceco.Modules.Budgets.IntegrationEvents.MonthlyBudgets;

public sealed class ExpenseRemovedFromMonthlyBudgetIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    string? investmentStrategyId,
    string monthlyBudgetExpenseId)
    : IntegrationEvent(id, occurredOnUtc)
{
    public string? InvestmentStrategyId { get; init; } = investmentStrategyId;
    public string MonthlyBudgetExpenseId { get; init; } = monthlyBudgetExpenseId;
}

