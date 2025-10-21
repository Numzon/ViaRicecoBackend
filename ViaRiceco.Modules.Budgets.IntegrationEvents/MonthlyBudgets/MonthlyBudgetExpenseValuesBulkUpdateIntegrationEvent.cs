using ViaRiceco.Common.Application.EventBus;

namespace ViaRiceco.Modules.Budgets.IntegrationEvents.MonthlyBudgets;

public sealed class MonthlyBudgetExpenseValuesBulkUpdateIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    string? investmentStrategyId,
    IReadOnlyCollection<ExpenseValueUpdateIntegrationModel> expenseValueUpdates)
    : IntegrationEvent(id, occurredOnUtc)
{
    public string? InvestmentStrategyId { get; init; } = investmentStrategyId;
    public IReadOnlyCollection<ExpenseValueUpdateIntegrationModel> ExpenseValueUpdates { get; init; } = expenseValueUpdates;
}

public sealed record ExpenseValueUpdateIntegrationModel(
    string MonthlyBudgetExpenseId, 
    decimal? Value);
