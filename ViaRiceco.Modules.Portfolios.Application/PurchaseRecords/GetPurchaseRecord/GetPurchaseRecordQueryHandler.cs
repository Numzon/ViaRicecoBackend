using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.PurchaseRecords.Models;
using ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;
using ViaRiceco.Modules.Portfolios.Domain.Investments;

namespace ViaRiceco.Modules.Portfolios.Application.PurchaseRecords.GetPurchaseRecord;

public sealed record GetPurchaseRecordQuery(string InvestmentStrategyId, string InvestmentId, string Id) : IQuery<PurchaseRecordDto>;

internal sealed class GetPurchaseRecordQueryHandler(
    IPurchaseRecordRepository repository,
    IInvestmentStrategyRepository investmentStrategyRepository,
    IInvestmentRepository investmentRepository)
    : IQueryHandler<GetPurchaseRecordQuery, PurchaseRecordDto>
{
    public async Task<Result<PurchaseRecordDto>> Handle(GetPurchaseRecordQuery request, CancellationToken cancellationToken)
    {
        InvestmentStrategy? investmentStrategy = await investmentStrategyRepository.GetAsync(request.InvestmentStrategyId, cancellationToken);
        if (investmentStrategy is null)
        {
            return Result.Failure<PurchaseRecordDto>(InvestmentStrategyErrors.NotFound(request.InvestmentStrategyId));
        }

        Investment? investment = await investmentRepository.GetAsync(request.InvestmentId, cancellationToken);
        if (investment is null)
        {
            return Result.Failure<PurchaseRecordDto>(InvestmentErrors.NotFound(request.InvestmentId));
        }

        if (investment.InvestmentStrategyId != request.InvestmentStrategyId)
        {
            return Result.Failure<PurchaseRecordDto>(InvestmentErrors.NotFound(request.InvestmentId));
        }

        PurchaseRecord? purchaseRecord = await repository.GetAsync(request.Id, cancellationToken);
        if (purchaseRecord is null)
        {
            return Result.Failure<PurchaseRecordDto>(PurchaseRecordErrors.NotFound(request.Id));
        }

        if (purchaseRecord.InvestmentId != request.InvestmentId)
        {
            return Result.Failure<PurchaseRecordDto>(PurchaseRecordErrors.NotFound(request.Id));
        }

        var dto = new PurchaseRecordDto(
            purchaseRecord.Id,
            purchaseRecord.PurchaseDate,
            purchaseRecord.Amount,
            purchaseRecord.PricePerUnit,
            purchaseRecord.TotalPrice,
            purchaseRecord.CurrencyId,
            purchaseRecord.InvestmentId);

        return dto;
    }
}

[UsedImplicitly]
internal sealed class GetPurchaseRecordQueryValidator : AbstractValidator<GetPurchaseRecordQuery>
{
    public GetPurchaseRecordQueryValidator()
    {
        RuleFor(x => x.InvestmentStrategyId)
            .NotEmpty()
            .WithMessage("Investment strategy ID is required");

        RuleFor(x => x.InvestmentId)
            .NotEmpty()
            .WithMessage("Investment ID is required");

        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Purchase record ID is required");
    }
}
