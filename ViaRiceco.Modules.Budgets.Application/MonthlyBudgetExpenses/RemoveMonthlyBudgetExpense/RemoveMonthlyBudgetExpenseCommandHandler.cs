using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Application.MonthlyBudgetExpenses.Models;
using ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.Models;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgetExpenses;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

namespace ViaRiceco.Modules.Budgets.Application.MonthlyBudgetExpenses.RemoveMonthlyBudgetExpense;

public sealed record RemoveMonthlyBudgetExpenseCommand(
    string MonthlyBudgetId, 
    string MonthlyBudgetExpenseId) : ICommand<MonthlyBudgetDto>;

internal sealed class RemoveMonthlyBudgetExpenseCommandHandler(
    IMonthlyBudgetRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<RemoveMonthlyBudgetExpenseCommand, MonthlyBudgetDto>
{
    public async Task<Result<MonthlyBudgetDto>> Handle(RemoveMonthlyBudgetExpenseCommand request, CancellationToken cancellationToken)
    {
        MonthlyBudget? monthlyBudget = await repository.GetAsync(request.MonthlyBudgetId, cancellationToken);

        if (monthlyBudget is null)
        {
            return Result.Failure<MonthlyBudgetDto>(MonthlyBudgetErrors.NotFound(request.MonthlyBudgetId));
        }

        Result result = monthlyBudget.RemoveExpense(request.MonthlyBudgetExpenseId, timeProvider.UtcNow());

        if (result.IsFailure)
        {
            return Result.Failure<MonthlyBudgetDto>(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new MonthlyBudgetDto(
            monthlyBudget.Id,
            monthlyBudget.SettlementPeriodId,
            monthlyBudget.Month,
            monthlyBudget.Year,
            monthlyBudget.IsDraft,
            monthlyBudget.NetValue,
            monthlyBudget.Expenses.Select(expense => new MonthlyBudgetExpenseDto(
                expense.Id,
                expense.MonthlyBudgetId,
                expense.ExpenseId,
                expense.ExpenseName,
                expense.ExpenseTypeId,
                expense.ExpenseTypeName,
                expense.Value,
                expense.CreatedAtUtc,
                expense.UpdatedAtUtc)).ToList(),
            monthlyBudget.TotalBudgetedAmount,
            monthlyBudget.CreatedAtUtc,
            monthlyBudget.UpdatedAtUtc);

        return dto;
    }
}

[UsedImplicitly]
internal sealed class RemoveMonthlyBudgetExpenseCommandValidator : AbstractValidator<RemoveMonthlyBudgetExpenseCommand>
{
    public RemoveMonthlyBudgetExpenseCommandValidator()
    {
        RuleFor(x => x.MonthlyBudgetId)
            .NotEmpty()
            .WithMessage("Monthly budget ID is required");

        RuleFor(x => x.MonthlyBudgetExpenseId)
            .NotEmpty()
            .WithMessage("Monthly budget expense ID is required");
    }
}
