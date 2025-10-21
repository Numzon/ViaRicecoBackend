using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

namespace ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.FinalizeMonthlyBudget;

public sealed record FinalizeMonthlyBudgetCommand(string Id) : ICommand;

internal sealed class FinalizeMonthlyBudgetCommandHandler(
    IMonthlyBudgetRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<FinalizeMonthlyBudgetCommand>
{
    public async Task<Result> Handle(FinalizeMonthlyBudgetCommand request,
        CancellationToken cancellationToken)
    {
        MonthlyBudget? monthlyBudget = await repository.GetAsync(request.Id, cancellationToken);

        if (monthlyBudget is null)
        {
            return Result.Failure(MonthlyBudgetErrors.NotFound(request.Id));
        }

        Result result = monthlyBudget.Finalize(timeProvider.UtcNow());
        if (!result.IsSuccess)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

[UsedImplicitly]
internal sealed class FinalizeMonthlyBudgetCommandValidator : AbstractValidator<FinalizeMonthlyBudgetCommand>
{
    public FinalizeMonthlyBudgetCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Monthly budget ID is required");
    }
}
