using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Modules.Accounting.IntegrationEvents.SettlementPeriods;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Domain.Expenses;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

namespace ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.SettlementPeriodCreated;

public sealed class SettlementPeriodCreatedIntegrationEventHandler(
    IMonthlyBudgetRepository monthlyBudgetRepository,
    IExpenseRepository expenseRepository,
    IExpenseTypeRepository expenseTypeRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IntegrationEventHandler<SettlementPeriodCreatedIntegrationEvent>
{
    public override async Task Handle(SettlementPeriodCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        bool exists = await monthlyBudgetRepository.ExistsForSettlementPeriodAsync(
            integrationEvent.SettlementPeriodId, cancellationToken);

        if (exists)
        {
            return; 
        }
        
        IReadOnlyCollection<Expense> activeExpenses = await expenseRepository
            .GetActiveExpensesAsync(cancellationToken);
        
        var expenseTypeIds = activeExpenses.Select(e => e.ExpenseTypeId).Distinct().ToList();
        IReadOnlyCollection<ExpenseType> expenseTypes = await expenseTypeRepository
            .GetByIdsAsync(expenseTypeIds, cancellationToken);
        
        var expenseDataList = activeExpenses
            .Join(expenseTypes, 
                expense => expense.ExpenseTypeId, 
                expenseType => expenseType.Id,
                ExpenseData.From)
            .ToList();
        
        var monthlyBudget = MonthlyBudget.CreateFromSettlementPeriod(
            integrationEvent.SettlementPeriodId,
            integrationEvent.Month,
            integrationEvent.Year,
            expenseDataList,
            timeProvider.UtcNow());

        monthlyBudgetRepository.Insert(monthlyBudget);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
