using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Modules.Budgets.IntegrationEvents.MonthlyBudgets;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Domain.InvestedCashRecords;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

namespace ViaRiceco.Modules.Portfolios.Presentation.InvestedCashRecords;

internal sealed class ExpenseRemovedFromMonthlyBudgetIntegrationEventHandler(
    IInvestedCashRecordRepository investedCashRecordRepository,
    IInvestmentStrategyRepository investmentStrategyRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IntegrationEventHandler<ExpenseRemovedFromMonthlyBudgetIntegrationEvent>
{
    public override async Task Handle(ExpenseRemovedFromMonthlyBudgetIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        InvestedCashRecord? investedCashRecord = await investedCashRecordRepository
            .GetByMonthlyBudgetExpenseIdAsync(integrationEvent.MonthlyBudgetExpenseId, cancellationToken);
        
        if (investedCashRecord == null)
        {
            return;
        }
        
        InvestmentStrategy? strategy =
            await investmentStrategyRepository.GetAsync(investedCashRecord.InvestmentStrategyId, cancellationToken);

        if (strategy == null)
        {
            return;
        }

        strategy.RemoveInvestedCashRecord(integrationEvent.MonthlyBudgetExpenseId, timeProvider.UtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

