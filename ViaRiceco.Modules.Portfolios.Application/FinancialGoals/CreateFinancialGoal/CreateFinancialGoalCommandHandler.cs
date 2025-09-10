using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Application.FinancialGoals.Models;
using ViaRiceco.Modules.Portfolios.Domain.FinancialGoals;

namespace ViaRiceco.Modules.Portfolios.Application.FinancialGoals.CreateFinancialGoal;

public sealed record CreateFinancialGoalCommand(string Name, string? ParentId) : ICommand<FinancialGoalDto>;

internal sealed class CreateFinancialGoalCommandHandler(
    IFinancialGoalRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateFinancialGoalCommand, FinancialGoalDto>
{
    public async Task<Result<FinancialGoalDto>> Handle(CreateFinancialGoalCommand request, CancellationToken cancellationToken)
    {
        // If ParentId is provided, validate it exists
        if (request.ParentId is not null)
        {
            FinancialGoal? parent = await repository.GetAsync(request.ParentId, cancellationToken);
            if (parent is null)
            {
                return Result.Failure<FinancialGoalDto>(FinancialGoalErrors.ParentNotFound(request.ParentId));
            }
        }

        Result<FinancialGoal> result = FinancialGoal.Create(request.Name, request.ParentId, timeProvider.UtcNow());
        
        if (result.IsFailure)
        {
            return Result.Failure<FinancialGoalDto>(result.Error);
        }

        FinancialGoal financialGoal = result.Value;
        repository.Insert(financialGoal);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new FinancialGoalDto(financialGoal.Id, financialGoal.Name, financialGoal.ParentId);

        return dto;
    }
}

[UsedImplicitly]
internal sealed class CreateFinancialGoalCommandValidator : AbstractValidator<CreateFinancialGoalCommand>
{
    public CreateFinancialGoalCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Financial goal name is required");
    }
}
