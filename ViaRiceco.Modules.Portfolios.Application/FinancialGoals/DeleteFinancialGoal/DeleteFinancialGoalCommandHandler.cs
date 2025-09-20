using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Domain.FinancialGoals;

namespace ViaRiceco.Modules.Portfolios.Application.FinancialGoals.DeleteFinancialGoal;

public sealed record DeleteFinancialGoalCommand(string Id) : ICommand;

internal sealed class DeleteFinancialGoalCommandHandler(
    IFinancialGoalRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteFinancialGoalCommand>
{
    public async Task<Result> Handle(DeleteFinancialGoalCommand request, CancellationToken cancellationToken)
    {
        FinancialGoal? financialGoal = await repository.GetAsync(request.Id, cancellationToken);

        if (financialGoal is null)
        {
            return Result.Failure(FinancialGoalErrors.NotFound(request.Id));
        }
        
        bool hasChildren = await repository.HasChildrenAsync(request.Id, cancellationToken);
        if (hasChildren)
        {
            return Result.Failure(FinancialGoalErrors.CannotDeleteGoalWithChildren(request.Id));
        }
        
        repository.Delete(financialGoal);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

[UsedImplicitly]
internal sealed class DeleteFinancialGoalCommandValidator : AbstractValidator<DeleteFinancialGoalCommand>
{
    public DeleteFinancialGoalCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Financial goal ID is required");
    }
}
