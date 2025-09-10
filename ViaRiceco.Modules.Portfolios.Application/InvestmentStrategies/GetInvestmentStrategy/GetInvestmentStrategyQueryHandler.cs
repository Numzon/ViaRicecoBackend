using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.Models;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

namespace ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.GetInvestmentStrategy;

public sealed record GetInvestmentStrategyQuery(string Id) : IQuery<InvestmentStrategyDto>;

internal sealed class GetInvestmentStrategyQueryHandler(IInvestmentStrategyRepository repository)
    : IQueryHandler<GetInvestmentStrategyQuery, InvestmentStrategyDto>
{
    public async Task<Result<InvestmentStrategyDto>> Handle(GetInvestmentStrategyQuery request, CancellationToken cancellationToken)
    {
        InvestmentStrategy? strategy = await repository.GetAsync(request.Id, cancellationToken);

        if (strategy is null)
        {
            return Result.Failure<InvestmentStrategyDto>(InvestmentStrategyErrors.NotFound(request.Id));
        }

        var dto = new InvestmentStrategyDto(
            strategy.Id,
            strategy.FinancialGoalId,
            strategy.InvestmentStrategyTypeId,
            strategy.UninvestedAmount,
            strategy.TotalInvestedAmount,
            strategy.TotalCurrentAmount,
            strategy.TotalAmount,
            strategy.Investments.Select(inv => new InvestmentSummaryDto(
                inv.Id,
                inv.Name,
                inv.ModelPortfolioPercentage,
                inv.RealPortfolioPercentage,
                inv.InvestedAmount,
                inv.CurrentAmount)).ToList());

        return dto;
    }
}

[UsedImplicitly]
internal sealed class GetInvestmentStrategyQueryValidator : AbstractValidator<GetInvestmentStrategyQuery>
{
    public GetInvestmentStrategyQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Investment strategy ID is required");
    }
}
