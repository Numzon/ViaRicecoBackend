using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.Models;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

namespace ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.RemoveInvestmentFromStrategy;

public sealed record RemoveInvestmentFromStrategyCommand(
    string InvestmentStrategyId,
    string InvestmentId) : ICommand<InvestmentStrategyDto>;

internal sealed class RemoveInvestmentFromStrategyCommandHandler(
    IInvestmentStrategyRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<RemoveInvestmentFromStrategyCommand, InvestmentStrategyDto>
{
    public async Task<Result<InvestmentStrategyDto>> Handle(RemoveInvestmentFromStrategyCommand request, CancellationToken cancellationToken)
    {
        InvestmentStrategy? strategy = await repository.GetAsync(request.InvestmentStrategyId, cancellationToken);

        if (strategy is null)
        {
            return Result.Failure<InvestmentStrategyDto>(InvestmentStrategyErrors.NotFound(request.InvestmentStrategyId));
        }

        Result result = strategy.RemoveInvestment(request.InvestmentId, timeProvider.UtcNow());

        if (result.IsFailure)
        {
            return Result.Failure<InvestmentStrategyDto>(result.Error);
        }

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
internal sealed class RemoveInvestmentFromStrategyCommandValidator : AbstractValidator<RemoveInvestmentFromStrategyCommand>
{
    public RemoveInvestmentFromStrategyCommandValidator()
    {
        RuleFor(x => x.InvestmentStrategyId)
            .NotEmpty()
            .WithMessage("Investment strategy ID is required");

        RuleFor(x => x.InvestmentId)
            .NotEmpty()
            .WithMessage("Investment ID is required");
    }
}
