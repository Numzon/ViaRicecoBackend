using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Application.FinancialGoals.Models;
using ViaRiceco.Modules.Portfolios.Domain.FinancialGoals;

namespace ViaRiceco.Modules.Portfolios.Application.FinancialGoals.UpdateFinancialGoal;

public sealed record UpdateFinancialGoalCommand(string Id, string Name, string? ParentId) : ICommand<FinancialGoalDto>;

internal sealed class UpdateFinancialGoalCommandHandler(
    IFinancialGoalRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateFinancialGoalCommand, FinancialGoalDto>
{
    public async Task<Result<FinancialGoalDto>> Handle(UpdateFinancialGoalCommand request, CancellationToken cancellationToken)
    {
        FinancialGoal? financialGoal = await repository.GetAsync(request.Id, cancellationToken);

        if (financialGoal is null)
        {
            return Result.Failure<FinancialGoalDto>(FinancialGoalErrors.NotFound(request.Id));
        }

        if (FinancialGoalSpecification.RequiresParentValidation(financialGoal, request.ParentId))
        {
            FinancialGoal? parent = await repository.GetAsync(request.ParentId!, cancellationToken);
            if (parent is null)
            {
                return Result.Failure<FinancialGoalDto>(FinancialGoalErrors.ParentNotFound(request.ParentId!));
            }

            bool hasCircularReference = await WouldCreateCircularReferenceAsync(request.Id, request.ParentId!, cancellationToken);
            if (hasCircularReference)
            {
                return Result.Failure<FinancialGoalDto>(FinancialGoalErrors.CircularReference(request.Id, request.ParentId!));
            }
        }

        Result result = financialGoal.Update(request.Name, request.ParentId, timeProvider.UtcNow());

        if (result.IsFailure)
        {
            return Result.Failure<FinancialGoalDto>(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new FinancialGoalDto(financialGoal.Id, financialGoal.Name, financialGoal.ParentId);

        return dto;
    }
    
    private async Task<bool> WouldCreateCircularReferenceAsync(string goalId, string proposedParentId, CancellationToken cancellationToken)
    {
        if (goalId == proposedParentId)
        {
            return true;
        }

        const int maxDepth = 100; 
        var visitedGoals = new HashSet<string> { goalId }; 
        string? currentParentId = proposedParentId;
        int depth = 0;

        while (currentParentId is not null && depth < maxDepth)
        {
            if (!visitedGoals.Add(currentParentId))
            {
                return true;
            }

            FinancialGoal? currentParent = await repository.GetAsync(currentParentId, cancellationToken);
            if (currentParent is null)
            {
                break;
            }

            currentParentId = currentParent.ParentId;
            depth++;
        }

        return false; 
    }
}

[UsedImplicitly]
internal sealed class UpdateFinancialGoalCommandValidator : AbstractValidator<UpdateFinancialGoalCommand>
{
    public UpdateFinancialGoalCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Financial goal ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Financial goal name is required");
    }
}
