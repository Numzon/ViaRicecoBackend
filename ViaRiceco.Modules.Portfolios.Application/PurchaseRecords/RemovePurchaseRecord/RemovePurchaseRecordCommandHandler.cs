using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

namespace ViaRiceco.Modules.Portfolios.Application.PurchaseRecords.RemovePurchaseRecord;

public sealed record RemovePurchaseRecordCommand(
    string InvestmentStrategyId,
    string InvestmentId,
    string PurchaseRecordId) : ICommand;

internal sealed class RemovePurchaseRecordCommandHandler(
    IInvestmentStrategyRepository investmentStrategyRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<RemovePurchaseRecordCommand>
{
    public async Task<Result> Handle(RemovePurchaseRecordCommand request,
        CancellationToken cancellationToken)
    {
        InvestmentStrategy? investmentStrategy =
            await investmentStrategyRepository.GetAsync(request.InvestmentStrategyId, cancellationToken);
        if (investmentStrategy is null)
        {
            return Result.Failure(InvestmentStrategyErrors.NotFound(request.InvestmentStrategyId));
        }

        Result removeResult = investmentStrategy.RemovePurchaseRecordFromInvestment(request.InvestmentId,
            request.PurchaseRecordId, timeProvider.UtcNow());

        if (removeResult.IsFailure)
        {
            return removeResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

[UsedImplicitly]
internal sealed class
    RemovePurchaseRecordFromInvestmentCommandValidator : AbstractValidator<RemovePurchaseRecordCommand>
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
