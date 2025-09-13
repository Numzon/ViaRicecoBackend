using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Domain.Investments;

namespace ViaRiceco.Modules.Portfolios.Application.Investments.RemovePurchaseRecordFromInvestment;

public sealed record RemovePurchaseRecordFromInvestmentCommand(
    string InvestmentId,
    string PurchaseRecordId) : ICommand;

internal sealed class RemovePurchaseRecordFromInvestmentCommandHandler(
    IInvestmentRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<RemovePurchaseRecordFromInvestmentCommand>
{
    public async Task<Result> Handle(RemovePurchaseRecordFromInvestmentCommand request, CancellationToken cancellationToken)
    {
        Investment? investment = await repository.GetAsync(request.InvestmentId, cancellationToken);

        if (investment is null)
        {
            return Result.Failure(InvestmentErrors.NotFound(request.InvestmentId));
        }

        Result removeResult = investment.RemovePurchaseRecord(request.PurchaseRecordId, timeProvider.GetUtcNow().DateTime);

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
        RuleFor(x => x.InvestmentId)
            .NotEmpty()
            .WithMessage("Investment ID is required");

        RuleFor(x => x.PurchaseRecordId)
            .NotEmpty()
            .WithMessage("Purchase record ID is required");
    }
}
