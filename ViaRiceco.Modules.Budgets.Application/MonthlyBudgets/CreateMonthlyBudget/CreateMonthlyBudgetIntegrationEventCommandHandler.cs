using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Domain.Expenses;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

namespace ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.CreateMonthlyBudget;

public sealed record CreateMonthlyBudgetIntegrationEventCommand(string SettlementPeriodId, int Month, int Year, decimal NetAmount) : ICommand;

public sealed class CreateMonthlyBudgetIntegrationEventCommandHandler(
    IMonthlyBudgetRepository monthlyBudgetRepository,
    IExpenseRepository expenseRepository,
    IExpenseTypeRepository expenseTypeRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<CreateMonthlyBudgetIntegrationEventCommand>
{
    public async Task<Result> Handle(CreateMonthlyBudgetIntegrationEventCommand request, CancellationToken cancellationToken)
    {
        bool exists = await monthlyBudgetRepository.ExistsForSettlementPeriodAsync(
            request.SettlementPeriodId, cancellationToken);
        
        if (exists)
        {
            return Result.Failure<MonthlyBudget>(MonthlyBudgetErrors.AlreadyExistsForPeriod(request.Year, request.Month));
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
            request.SettlementPeriodId,
            request.Month,
            request.Year,
            request.NetAmount,
            expenseDataList,
            timeProvider.UtcNow());

        monthlyBudgetRepository.Insert(monthlyBudget);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
