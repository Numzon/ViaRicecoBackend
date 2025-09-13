using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Application.PurchaseRecords.Models;
using ViaRiceco.Modules.Portfolios.Domain.Currencies;
using ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;

namespace ViaRiceco.Modules.Portfolios.Application.PurchaseRecords.UpdatePurchaseRecord;

public sealed record UpdatePurchaseRecordCommand(
    string Id,
    DateTime PurchaseDate,
    decimal Amount,
    decimal PricePerUnit,
    string CurrencyId) : ICommand<PurchaseRecordDto>;

internal sealed class UpdatePurchaseRecordCommandHandler(
    IPurchaseRecordRepository repository,
    ICurrencyRepository currencyRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<UpdatePurchaseRecordCommand, PurchaseRecordDto>
{
    public async Task<Result<PurchaseRecordDto>> Handle(UpdatePurchaseRecordCommand request, CancellationToken cancellationToken)
    {
        PurchaseRecord? purchaseRecord = await repository.GetAsync(request.Id, cancellationToken);

        if (purchaseRecord is null)
        {
            return Result.Failure<PurchaseRecordDto>(PurchaseRecordErrors.NotFound(request.Id));
        }

        Currency? currency = await currencyRepository.GetAsync(request.CurrencyId, cancellationToken);

        if (currency is null)
        {
            return Result.Failure<PurchaseRecordDto>(CurrencyErrors.NotFound(request.CurrencyId));
        }

        purchaseRecord.Update(
            request.PurchaseDate,
            request.Amount,
            request.PricePerUnit,
            request.CurrencyId,
            timeProvider.GetUtcNow().DateTime);

        await unitOfWork.SaveChangesAsync(cancellationToken);

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
internal sealed class UpdatePurchaseRecordCommandValidator : AbstractValidator<UpdatePurchaseRecordCommand>
{
    public UpdatePurchaseRecordCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Purchase record ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero");

        RuleFor(x => x.PricePerUnit)
            .GreaterThan(0)
            .WithMessage("Price per unit must be greater than zero");

        RuleFor(x => x.CurrencyId)
            .NotEmpty()
            .WithMessage("Currency ID is required");

        RuleFor(x => x.PurchaseDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Purchase date cannot be in the future");
    }
}
