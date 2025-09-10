using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.Models;
using ViaRiceco.Modules.Portfolios.Domain.Investments;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

namespace ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.AddInvestmentToStrategy;

public sealed record AddInvestmentToStrategyCommand(
    string InvestmentStrategyId,
    string InvestmentName,
    decimal ModelPortfolioPercentage) : ICommand<InvestmentStrategyDto>;

internal sealed class AddInvestmentToStrategyCommandHandler(
    IInvestmentStrategyRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<AddInvestmentToStrategyCommand, InvestmentStrategyDto>
{
    public async Task<Result<InvestmentStrategyDto>> Handle(AddInvestmentToStrategyCommand request, CancellationToken cancellationToken)
    {
        InvestmentStrategy? strategy = await repository.GetAsync(request.InvestmentStrategyId, cancellationToken);

        if (strategy is null)
        {
            return Result.Failure<InvestmentStrategyDto>(InvestmentStrategyErrors.NotFound(request.InvestmentStrategyId));
        }

        Result<Investment> addResult = strategy.AddInvestment(
            request.InvestmentName,
            request.ModelPortfolioPercentage,
            timeProvider.UtcNow());

        if (addResult.IsFailure)
        {
            return Result.Failure<InvestmentStrategyDto>(addResult.Error);
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
internal sealed class AddInvestmentToStrategyCommandValidator : AbstractValidator<AddInvestmentToStrategyCommand>
{
    public AddInvestmentToStrategyCommandValidator()
    {
        RuleFor(x => x.InvestmentStrategyId)
            .NotEmpty()
            .WithMessage("Investment strategy ID is required");

        RuleFor(x => x.InvestmentName)
            .NotEmpty()
            .WithMessage("Investment name is required");

        RuleFor(x => x.ModelPortfolioPercentage)
            .InclusiveBetween(0, 100)
            .WithMessage("Model portfolio percentage must be between 0 and 100");
    }
}
