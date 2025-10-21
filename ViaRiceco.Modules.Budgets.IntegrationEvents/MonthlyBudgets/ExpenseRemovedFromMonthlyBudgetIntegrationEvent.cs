using ViaRiceco.Common.Application.EventBus;

namespace ViaRiceco.Modules.Budgets.IntegrationEvents.MonthlyBudgets;

public sealed class ExpenseRemovedFromMonthlyBudgetIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    string monthlyBudgetExpenseId)
    : IntegrationEvent(id, occurredOnUtc)
{
    public string MonthlyBudgetExpenseId { get; init; } = monthlyBudgetExpenseId;
}

