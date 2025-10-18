using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Application.Messaging;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgetExpenses;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;
using ViaRiceco.Modules.Budgets.IntegrationEvents.MonthlyBudgets;

namespace ViaRiceco.Modules.Budgets.Application.MonthlyBudgetExpenses.ExpenseRemovedFromMonthlyBudget;

internal sealed class ExpenseRemovedFromMonthlyBudgetDomainEventHandler(
    IEventBus eventBus,
    IMonthlyBudgetExpenseRepository monthlyBudgetExpenseRepository)
    : DomainEventHandler<ExpenseRemovedFromMonthlyBudgetDomainEvent>
{
    public override async Task Handle(ExpenseRemovedFromMonthlyBudgetDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<MonthlyBudgetExpenseWithExpense> investmentExpenses =
            await monthlyBudgetExpenseRepository.GetByIdsWithInvestmentExpensesAsync([domainEvent.ExpenseId], cancellationToken);

        if (investmentExpenses.Count == 0)
        {
            return;
        }

        MonthlyBudgetExpenseWithExpense investmentExpense = investmentExpenses.First();

        var integrationEvent = new ExpenseRemovedFromMonthlyBudgetIntegrationEvent(
            domainEvent.Id,
            domainEvent.OccurredOnUtc,
            investmentExpense.InvestmentStrategyId,
            investmentExpense.MonthlyBudgetExpenseId);

        await eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}

