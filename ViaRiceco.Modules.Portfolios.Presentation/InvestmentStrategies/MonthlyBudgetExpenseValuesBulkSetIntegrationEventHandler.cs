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
        string investmentStrategyId = integrationEvent.InvestmentStrategyId;

        await UpdateInvestedCashHistoryRecordsAsync(integrationEvent.ExpenseValueUpdates, investmentStrategyId, now, cancellationToken);
        
        // Notify that invested cash histories were updated - triggers single recalculation
        InvestmentStrategy? strategy = await investmentStrategyRepository.GetAsync(investmentStrategyId, cancellationToken);
        strategy?.NotifyInvestedCashHistoriesUpdated(now);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
    
    private async Task UpdateInvestedCashHistoryRecordsAsync(
        IReadOnlyCollection<ExpenseValueUpdateIntegrationModel> expenseUpdates,
        string investmentStrategyId,
        DateTime updatedAtUtc,
        CancellationToken cancellationToken)
    {
        foreach (ExpenseValueUpdateIntegrationModel expenseUpdate in expenseUpdates)
        {
            InvestedCashHistory? existingRecord = await investedCashHistoryRepository
                .GetByInvestmentStrategyAndMonthlyBudgetExpenseAsync(
                    investmentStrategyId, 
                    expenseUpdate.MonthlyBudgetExpenseId, 
                    cancellationToken);

            decimal amount = expenseUpdate.Value ?? 0m;

            if (existingRecord != null)
            {
                existingRecord.UpdateAmount(amount, updatedAtUtc);
            }
            else
            {
                var newRecord = InvestedCashHistory.Create(
                    investmentStrategyId,
                    expenseUpdate.MonthlyBudgetExpenseId,
                    amount,
                    updatedAtUtc);
            
                investedCashHistoryRepository.Insert(newRecord);
            }
        }
    }
}
