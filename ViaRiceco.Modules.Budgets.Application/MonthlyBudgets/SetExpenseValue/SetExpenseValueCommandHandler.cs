using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

namespace ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.SetExpenseValue;

public sealed record SetExpenseValueCommand(string MonthlyBudgetId, string MonthlyBudgetExpenseId, decimal? Value) : ICommand;

internal sealed class SetExpenseValueCommandHandler(
    IMonthlyBudgetRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<SetExpenseValueCommand>
{
    public async Task<Result> Handle(SetExpenseValueCommand request, CancellationToken cancellationToken)
    {
        MonthlyBudget? monthlyBudget = await repository.GetAsync(request.MonthlyBudgetId, cancellationToken);

        if (monthlyBudget is null)
        {
            return Result.Failure(MonthlyBudgetErrors.NotFound(request.MonthlyBudgetId));
        }

        Result result = monthlyBudget.SetExpenseValueById(request.MonthlyBudgetExpenseId, request.Value, timeProvider.UtcNow());
        if (!result.IsSuccess)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

[UsedImplicitly]
internal sealed class SetExpenseValueCommandValidator : AbstractValidator<SetExpenseValueCommand>
{
    public SetExpenseValueCommandValidator()
    {
        RuleFor(x => x.MonthlyBudgetId)
            .NotEmpty()
            .WithMessage("Monthly budget ID is required");

        RuleFor(x => x.MonthlyBudgetExpenseId)
            .NotEmpty()
            .WithMessage("Monthly budget expense ID is required");

        RuleFor(x => x.Value)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Value.HasValue)
            .WithMessage("Expense value must be greater than or equal to 0");
    }
}
