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

        // If ParentId is provided and different, validate it exists and isn't circular
        if (request.ParentId is not null && request.ParentId != financialGoal.ParentId)
        {
            if (request.ParentId == request.Id)
            {
                return Result.Failure<FinancialGoalDto>(FinancialGoalErrors.CircularReference(request.Id, request.ParentId));
            }

            FinancialGoal? parent = await repository.GetAsync(request.ParentId, cancellationToken);
            if (parent is null)
            {
                return Result.Failure<FinancialGoalDto>(FinancialGoalErrors.ParentNotFound(request.ParentId));
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
