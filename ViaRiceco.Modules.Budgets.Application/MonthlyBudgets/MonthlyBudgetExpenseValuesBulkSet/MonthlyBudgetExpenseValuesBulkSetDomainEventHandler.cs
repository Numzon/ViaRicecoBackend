using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Application.Messaging;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgetExpenses;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;
using ViaRiceco.Modules.Budgets.IntegrationEvents.MonthlyBudgets;

namespace ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.MonthlyBudgetExpenseValuesBulkSet;

public sealed class MonthlyBudgetExpenseValuesBulkSetDomainEventHandler(
    IEventBus eventBus,
    IMonthlyBudgetExpenseRepository monthlyBudgetExpenseRepository) 
    : DomainEventHandler<MonthlyBudgetExpenseValuesBulkSetDomainEvent>
{
    public override async Task Handle(MonthlyBudgetExpenseValuesBulkSetDomainEvent domainEvent, 
        CancellationToken cancellationToken = default)
    {
        var monthlyBudgetExpenseIds = domainEvent.ExpenseValueUpdates
            .Select(update => update.MonthlyBudgetExpenseId)
            .ToList();

        IReadOnlyCollection<MonthlyBudgetExpenseWithExpense> investmentExpenses = 
            await monthlyBudgetExpenseRepository.GetByIdsWithInvestmentExpensesAsync(monthlyBudgetExpenseIds, cancellationToken);

        var valueUpdatesLookup = domainEvent.ExpenseValueUpdates.ToDictionary(
            update => update.MonthlyBudgetExpenseId, 
            update => update.Value);
        
        // Only publish integration event if there are investment expenses to process
        if (investmentExpenses.Count > 0)
        {
            // Get the single InvestmentStrategyId (all expenses will have the same one)
            string? investmentStrategyId = investmentExpenses.First().InvestmentStrategyId;

            // Build integration event data for Investment expenses
            var expenseValueUpdates = investmentExpenses
                .Select(expense => new ExpenseValueUpdateIntegrationModel(
                    expense.MonthlyBudgetExpenseId,
                    valueUpdatesLookup[expense.MonthlyBudgetExpenseId],
                    expense.ExpenseId))
                .ToList();

            var integrationEvent = new MonthlyBudgetExpenseValuesBulkSetIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.MonthlyBudgetId,
                investmentStrategyId,
                expenseValueUpdates);

            await eventBus.PublishAsync(integrationEvent, cancellationToken);
        }
    }
}
