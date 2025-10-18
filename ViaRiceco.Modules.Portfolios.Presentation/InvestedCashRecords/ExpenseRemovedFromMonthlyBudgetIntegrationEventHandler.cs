using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Modules.Budgets.IntegrationEvents.MonthlyBudgets;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

namespace ViaRiceco.Modules.Portfolios.Presentation.InvestedCashRecords;

internal sealed class ExpenseRemovedFromMonthlyBudgetIntegrationEventHandler(
    IInvestmentStrategyRepository investmentStrategyRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IntegrationEventHandler<ExpenseRemovedFromMonthlyBudgetIntegrationEvent>
{
    public override async Task Handle(ExpenseRemovedFromMonthlyBudgetIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(integrationEvent.InvestmentStrategyId))
        {
            return;
        }

        InvestmentStrategy? strategy =
            await investmentStrategyRepository.GetAsync(integrationEvent.InvestmentStrategyId, cancellationToken);

        if (strategy == null)
        {
            return;
        }

        strategy.RemoveInvestedCashRecord(integrationEvent.MonthlyBudgetExpenseId, timeProvider.UtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

