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

        if (investmentExpenses.Count > 0)
        {
            string? investmentStrategyId = investmentExpenses.First().InvestmentStrategyId;

            var expenseValueUpdates = investmentExpenses
                .Select(expense => new ExpenseValueUpdateIntegrationModel(
                    expense.MonthlyBudgetExpenseId,
                    expense.Value))
                .ToList();

            var integrationEvent = new MonthlyBudgetExpenseValuesBulkSetIntegrationEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                investmentStrategyId,
                expenseValueUpdates);

            await eventBus.PublishAsync(integrationEvent, cancellationToken);
        }
    }
}
