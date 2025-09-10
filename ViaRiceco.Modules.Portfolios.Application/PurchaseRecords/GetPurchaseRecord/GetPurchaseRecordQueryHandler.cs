using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions.Queries;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.PurchaseRecords.Models;
using ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;

namespace ViaRiceco.Modules.Portfolios.Application.PurchaseRecords.GetPurchaseRecord;

public sealed record GetPurchaseRecordQuery(string Id) : IQuery<PurchaseRecordDto>;

internal sealed class GetPurchaseRecordQueryHandler(IPurchaseRecordRepository repository)
    : IQueryHandler<GetPurchaseRecordQuery, PurchaseRecordDto>
{
    public async Task<Result<PurchaseRecordDto>> Handle(GetPurchaseRecordQuery request, CancellationToken cancellationToken)
    {
        PurchaseRecord? purchaseRecord = await repository.GetAsync(request.Id, cancellationToken);

        if (purchaseRecord is null)
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
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Purchase record ID is required");
    }
}
