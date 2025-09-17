using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.FinancialGoals.Models;
using ViaRiceco.Modules.Portfolios.Domain.FinancialGoals;

namespace ViaRiceco.Modules.Portfolios.Application.FinancialGoals.GetFinancialGoal;

public sealed record GetFinancialGoalQuery(string Id) : IQuery<FinancialGoalDto>;

internal sealed class GetFinancialGoalQueryHandler(IFinancialGoalRepository repository)
    : IQueryHandler<GetFinancialGoalQuery, FinancialGoalDto>
{
    public async Task<Result<FinancialGoalDto>> Handle(GetFinancialGoalQuery request, CancellationToken cancellationToken)
    {
        FinancialGoal? financialGoal = await repository.GetAsync(request.Id, cancellationToken);

        if (financialGoal is null)
        {
            return Result.Failure<FinancialGoalDto>(FinancialGoalErrors.NotFound(request.Id));
        }

        var dto = new FinancialGoalDto(financialGoal.Id, financialGoal.Name, financialGoal.ParentId);

        return dto;
    }
}

[UsedImplicitly]
internal sealed class GetFinancialGoalQueryValidator : AbstractValidator<GetFinancialGoalQuery>
{
    public GetFinancialGoalQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Financial goal ID is required");
    }
}
