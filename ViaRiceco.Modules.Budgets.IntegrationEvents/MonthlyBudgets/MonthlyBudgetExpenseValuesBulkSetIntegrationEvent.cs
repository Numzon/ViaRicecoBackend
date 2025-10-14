using ViaRiceco.Common.Application.EventBus;

namespace ViaRiceco.Modules.Budgets.IntegrationEvents.MonthlyBudgets;

public sealed class MonthlyBudgetExpenseValuesBulkSetIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    string monthlyBudgetId,
    string? investmentStrategyId,
    IReadOnlyCollection<ExpenseValueUpdateIntegrationModel> expenseValueUpdates)
    : IntegrationEvent(id, occurredOnUtc)
{
    public string MonthlyBudgetId { get; init; } = monthlyBudgetId;
    public string? InvestmentStrategyId { get; init; } = investmentStrategyId;
    public IReadOnlyCollection<ExpenseValueUpdateIntegrationModel> ExpenseValueUpdates { get; init; } = expenseValueUpdates;
}

public sealed record ExpenseValueUpdateIntegrationModel(
    string MonthlyBudgetExpenseId, 
    decimal? Value,
    string ExpenseId);
