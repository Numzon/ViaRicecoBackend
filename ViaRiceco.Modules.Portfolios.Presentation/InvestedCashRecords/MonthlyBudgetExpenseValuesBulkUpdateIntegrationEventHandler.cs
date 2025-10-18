using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Modules.Budgets.IntegrationEvents.MonthlyBudgets;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

namespace ViaRiceco.Modules.Portfolios.Presentation.InvestedCashRecords;

internal sealed class MonthlyBudgetExpenseValuesBulkUpdateIntegrationEventHandler(
    IInvestmentStrategyRepository investmentStrategyRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IntegrationEventHandler<MonthlyBudgetExpenseValuesBulkUpdateIntegrationEvent>
{
    public override async Task Handle(MonthlyBudgetExpenseValuesBulkUpdateIntegrationEvent integrationEvent,
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

        var keyValueParis =
            integrationEvent.ExpenseValueUpdates
                .Select(x => new KeyValuePair<string, decimal>(x.MonthlyBudgetExpenseId, x.Value ?? 0m))
                .ToList();

        strategy.UpdateInvestedCashRecords(keyValueParis, timeProvider.UtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
