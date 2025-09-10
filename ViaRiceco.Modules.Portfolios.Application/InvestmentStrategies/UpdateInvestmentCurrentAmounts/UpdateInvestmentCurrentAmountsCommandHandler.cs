using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.Models;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

namespace ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.UpdateInvestmentCurrentAmounts;

public sealed record UpdateInvestmentCurrentAmountsCommand(
    string InvestmentStrategyId,
    Dictionary<string, decimal> InvestmentCurrentAmounts) : ICommand<InvestmentStrategyDto>;

internal sealed class UpdateInvestmentCurrentAmountsCommandHandler(
    IInvestmentStrategyRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateInvestmentCurrentAmountsCommand, InvestmentStrategyDto>
{
    public async Task<Result<InvestmentStrategyDto>> Handle(UpdateInvestmentCurrentAmountsCommand request, CancellationToken cancellationToken)
    {
        InvestmentStrategy? strategy = await repository.GetAsync(request.InvestmentStrategyId, cancellationToken);

        if (strategy is null)
        {
            return Result.Failure<InvestmentStrategyDto>(InvestmentStrategyErrors.NotFound(request.InvestmentStrategyId));
        }

        strategy.UpdateInvestmentCurrentAmounts(request.InvestmentCurrentAmounts, timeProvider.UtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);

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
internal sealed class UpdateInvestmentCurrentAmountsCommandValidator : AbstractValidator<UpdateInvestmentCurrentAmountsCommand>
{
    public UpdateInvestmentCurrentAmountsCommandValidator()
    {
        RuleFor(x => x.InvestmentStrategyId)
            .NotEmpty()
            .WithMessage("Investment strategy ID is required");

        RuleFor(x => x.InvestmentCurrentAmounts)
            .NotNull()
            .NotEmpty()
            .WithMessage("Investment current amounts are required");

        RuleForEach(x => x.InvestmentCurrentAmounts.Values)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Each investment current amount must be greater than or equal to zero");
    }
}
