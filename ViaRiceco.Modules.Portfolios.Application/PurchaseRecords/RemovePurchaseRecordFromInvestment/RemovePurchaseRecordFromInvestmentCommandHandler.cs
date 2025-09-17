using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Domain.Investments;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

namespace ViaRiceco.Modules.Portfolios.Application.PurchaseRecords.RemovePurchaseRecordFromInvestment;

public sealed record RemovePurchaseRecordFromInvestmentCommand(
    string InvestmentStrategyId,
    string InvestmentId,
    string PurchaseRecordId) : ICommand;

internal sealed class RemovePurchaseRecordFromInvestmentCommandHandler(
    IInvestmentRepository repository,
    IInvestmentStrategyRepository investmentStrategyRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<RemovePurchaseRecordFromInvestmentCommand>
{
    public async Task<Result> Handle(RemovePurchaseRecordFromInvestmentCommand request, CancellationToken cancellationToken)
    {
        InvestmentStrategy? investmentStrategy = await investmentStrategyRepository.GetAsync(request.InvestmentStrategyId, cancellationToken);
        if (investmentStrategy is null)
        {
            return Result.Failure(InvestmentStrategyErrors.NotFound(request.InvestmentStrategyId));
        }

        Investment? investment = await repository.GetAsync(request.InvestmentId, cancellationToken);
        if (investment is null)
        {
            return Result.Failure(InvestmentErrors.NotFound(request.InvestmentId));
        }

        if (investment.InvestmentStrategyId != request.InvestmentStrategyId)
        {
            return Result.Failure(InvestmentErrors.NotFound(request.InvestmentId));
        }

        Result removeResult = investment.RemovePurchaseRecord(request.PurchaseRecordId, timeProvider.UtcNow());

        if (removeResult.IsFailure)
        {
            return removeResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

[UsedImplicitly]
internal sealed class RemovePurchaseRecordFromInvestmentCommandValidator : AbstractValidator<RemovePurchaseRecordFromInvestmentCommand>
{
    public RemovePurchaseRecordFromInvestmentCommandValidator()
    {
        RuleFor(x => x.InvestmentStrategyId)
            .NotEmpty()
            .WithMessage("Investment strategy ID is required");

        RuleFor(x => x.InvestmentId)
            .NotEmpty()
            .WithMessage("Investment ID is required");

        RuleFor(x => x.PurchaseRecordId)
            .NotEmpty()
            .WithMessage("Purchase record ID is required");
    }
}
