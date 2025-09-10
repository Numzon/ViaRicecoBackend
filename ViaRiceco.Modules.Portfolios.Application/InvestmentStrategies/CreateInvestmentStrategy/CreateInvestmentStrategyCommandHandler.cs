using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.Models;
using ViaRiceco.Modules.Portfolios.Domain.FinancialGoals;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentTypes;

namespace ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.CreateInvestmentStrategy;

public sealed record CreateInvestmentStrategyCommand(
    string FinancialGoalId,
    string InvestmentStrategyTypeId,
    decimal UninvestedAmount) : ICommand<InvestmentStrategyDto>;

internal sealed class CreateInvestmentStrategyCommandHandler(
    IInvestmentStrategyRepository repository,
    IFinancialGoalRepository financialGoalRepository,
    IInvestmentStrategyTypeRepository investmentStrategyTypeRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<CreateInvestmentStrategyCommand, InvestmentStrategyDto>
{
    public async Task<Result<InvestmentStrategyDto>> Handle(CreateInvestmentStrategyCommand request, CancellationToken cancellationToken)
    {
        // Validate FinancialGoal exists
        FinancialGoal? financialGoal = await financialGoalRepository.GetAsync(request.FinancialGoalId, cancellationToken);
        if (financialGoal is null)
        {
            return Result.Failure<InvestmentStrategyDto>(FinancialGoalErrors.NotFound(request.FinancialGoalId));
        }

        // Validate InvestmentStrategyType exists
        InvestmentStrategyType? strategyType = await investmentStrategyTypeRepository.GetAsync(request.InvestmentStrategyTypeId, cancellationToken);
        if (strategyType is null)
        {
            return Result.Failure<InvestmentStrategyDto>(InvestmentStrategyTypeErrors.NotFound(request.InvestmentStrategyTypeId));
        }

        var strategy = InvestmentStrategy.Create(
            request.FinancialGoalId,
            request.InvestmentStrategyTypeId,
            request.UninvestedAmount,
            timeProvider.UtcNow());

        repository.Insert(strategy);

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
internal sealed class CreateInvestmentStrategyCommandValidator : AbstractValidator<CreateInvestmentStrategyCommand>
{
    public CreateInvestmentStrategyCommandValidator()
    {
        RuleFor(x => x.FinancialGoalId)
            .NotEmpty()
            .WithMessage("Financial goal ID is required");

        RuleFor(x => x.InvestmentStrategyTypeId)
            .NotEmpty()
            .WithMessage("Investment strategy type ID is required");

        RuleFor(x => x.UninvestedAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Uninvested amount must be greater than or equal to zero");
    }
}
