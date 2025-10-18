using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Application.Messaging;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;
using ViaRiceco.Modules.Budgets.IntegrationEvents.MonthlyBudgets;

namespace ViaRiceco.Modules.Budgets.Application.MonthlyBudgetExpenses.ExpenseRemovedFromMonthlyBudget;

internal sealed class ExpenseRemovedFromMonthlyBudgetDomainEventHandler(IEventBus eventBus)
    : DomainEventHandler<ExpenseRemovedFromMonthlyBudgetDomainEvent>
{
    public override async Task Handle(ExpenseRemovedFromMonthlyBudgetDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {

        var integrationEvent = new ExpenseRemovedFromMonthlyBudgetIntegrationEvent(
            domainEvent.Id,
            domainEvent.OccurredOnUtc,
            domainEvent.MonthlyBudgetExpenseId);

        await eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}

