using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Modules.Budgets.IntegrationEvents.MonthlyBudgets;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Domain.InvestedCashHistories;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

namespace ViaRiceco.Modules.Portfolios.Presentation.InvestmentStrategies;

internal sealed class MonthlyBudgetExpenseValuesBulkSetIntegrationEventHandler(
    IInvestedCashHistoryRepository investedCashHistoryRepository,
    IInvestmentStrategyRepository investmentStrategyRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IntegrationEventHandler<MonthlyBudgetExpenseValuesBulkSetIntegrationEvent>
{
    public override async Task Handle(MonthlyBudgetExpenseValuesBulkSetIntegrationEvent integrationEvent, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(integrationEvent.InvestmentStrategyId))
        {
            return;
        }

        DateTime now = timeProvider.UtcNow();

        foreach (ExpenseValueUpdateIntegrationModel expenseUpdate in integrationEvent.ExpenseValueUpdates)
        {
            InvestedCashHistory existingInvestedCashHistory = await investedCashHistoryRepository
                .GetByInvestmentStrategyAndMonthlyBudgetExpenseAsync(
                    integrationEvent.InvestmentStrategyId, 
                    expenseUpdate.MonthlyBudgetExpenseId, 
                    cancellationToken);

            if (existingInvestedCashHistory != null)
            {
                existingInvestedCashHistory.UpdateAmount(expenseUpdate.Value ?? 0m, now);
            }
            else
            {
                var newInvestedCashHistory = InvestedCashHistory.Create(
                    integrationEvent.InvestmentStrategyId,
                    expenseUpdate.MonthlyBudgetExpenseId,
                    expenseUpdate.Value ?? 0m,
                    now);
                
                investedCashHistoryRepository.Insert(newInvestedCashHistory);
            }
        }

        decimal totalInvestedCash = await investedCashHistoryRepository
            .GetTotalInvestedCashByInvestmentStrategyAsync(integrationEvent.InvestmentStrategyId, cancellationToken);

        InvestmentStrategy? strategy = await investmentStrategyRepository
            .GetAsync(integrationEvent.InvestmentStrategyId, cancellationToken);

        if (strategy != null)
        {
            decimal uninvestedAmount = totalInvestedCash - strategy.TotalInvestedAmount;
            
            if (uninvestedAmount < 0)
            {
                uninvestedAmount = 0;
            }

            strategy.UpdateUninvestedAmount(uninvestedAmount, now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
