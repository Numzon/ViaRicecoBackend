using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

namespace ViaRiceco.Modules.Budgets.Application.MonthlyBudgetExpenses.BulkSetExpenseValues;

public sealed record BulkSetExpenseValuesCommand(string MonthlyBudgetId, IReadOnlyCollection<ExpenseValueUpdateDto> ExpenseValueUpdates) : ICommand;

public sealed record ExpenseValueUpdateDto(string MonthlyBudgetExpenseId, decimal? Value);

internal sealed class BulkSetExpenseValuesCommandHandler(
    IMonthlyBudgetRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<BulkSetExpenseValuesCommand>
{
    public async Task<Result> Handle(BulkSetExpenseValuesCommand request, CancellationToken cancellationToken)
    {
        MonthlyBudget? monthlyBudget = await repository.GetAsync(request.MonthlyBudgetId, cancellationToken);

        if (monthlyBudget is null)
        {
            return Result.Failure(MonthlyBudgetErrors.NotFound(request.MonthlyBudgetId));
        }
        
        var expenseValueUpdates = request.ExpenseValueUpdates
            .Select(dto => new ExpenseValueUpdate(dto.MonthlyBudgetExpenseId, dto.Value))
            .ToList();

        Result result = monthlyBudget.BulkSetExpenseValues(expenseValueUpdates, timeProvider.UtcNow());
        if (!result.IsSuccess)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

[UsedImplicitly]
internal sealed class BulkSetExpenseValuesCommandValidator : AbstractValidator<BulkSetExpenseValuesCommand>
{
    public BulkSetExpenseValuesCommandValidator()
    {
        RuleFor(x => x.MonthlyBudgetId)
            .NotEmpty()
            .WithMessage("Monthly budget ID is required");

        RuleFor(x => x.ExpenseValueUpdates)
            .NotEmpty()
            .WithMessage("At least one expense value update is required")
            .Must(updates => updates.Count <= 100)
            .WithMessage("Cannot update more than 100 expenses at once");

        RuleForEach(x => x.ExpenseValueUpdates)
            .ChildRules(expense =>
            {
                expense.RuleFor(e => e.MonthlyBudgetExpenseId)
                    .NotEmpty()
                    .WithMessage("Monthly budget expense ID is required");

                expense.RuleFor(e => e.Value)
                    .GreaterThanOrEqualTo(0)
                    .When(e => e.Value.HasValue)
                    .WithMessage("Expense value must be greater than or equal to 0");
            });
    }
}
