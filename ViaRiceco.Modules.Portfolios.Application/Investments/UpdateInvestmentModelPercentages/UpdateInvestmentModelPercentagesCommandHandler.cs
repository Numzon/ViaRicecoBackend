using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Application.InvestmentStrategies.Models;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

namespace ViaRiceco.Modules.Portfolios.Application.Investments.UpdateInvestmentModelPercentages;

public sealed record UpdateInvestmentModelPercentagesCommand(
    string InvestmentStrategyId,
    Dictionary<string, decimal> InvestmentPercentages) : ICommand<InvestmentStrategyDto>;

internal sealed class UpdateInvestmentModelPercentagesCommandHandler(
    IInvestmentStrategyRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateInvestmentModelPercentagesCommand, InvestmentStrategyDto>
{
    public async Task<Result<InvestmentStrategyDto>> Handle(UpdateInvestmentModelPercentagesCommand request, CancellationToken cancellationToken)
    {
        InvestmentStrategy? strategy = await repository.GetAsync(request.InvestmentStrategyId, cancellationToken);

        if (strategy is null)
        {
            return Result.Failure<InvestmentStrategyDto>(InvestmentStrategyErrors.NotFound(request.InvestmentStrategyId));
        }

        Result result = strategy.UpdateInvestmentsModelPercentages(request.InvestmentPercentages, timeProvider.UtcNow());

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
internal sealed class UpdateInvestmentModelPercentagesCommandValidator : AbstractValidator<UpdateInvestmentModelPercentagesCommand>
{
    public UpdateInvestmentModelPercentagesCommandValidator()
    {
        RuleFor(x => x.InvestmentStrategyId)
            .NotEmpty()
            .WithMessage("Investment strategy ID is required");

        RuleFor(x => x.InvestmentPercentages)
            .NotNull()
            .NotEmpty()
            .WithMessage("Investment percentages are required");

        RuleForEach(x => x.InvestmentPercentages.Values)
            .InclusiveBetween(0, 100)
            .WithMessage("Each investment percentage must be between 0 and 100");

        RuleFor(x => x.InvestmentPercentages.Values.Sum())
            .LessThanOrEqualTo(100)
            .WithMessage("Total investment percentages cannot exceed 100%");
    }
}
