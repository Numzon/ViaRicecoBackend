using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Application.PurchaseRecords.Models;
using ViaRiceco.Modules.Portfolios.Domain.Currencies;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;
using ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;

namespace ViaRiceco.Modules.Portfolios.Application.PurchaseRecords.UpdatePurchaseRecord;

public sealed record UpdatePurchaseRecordCommand(
    string InvestmentStrategyId,
    string InvestmentId,
    string PurchaseRecordId,
    DateTime PurchaseDate,
    decimal Amount,
    decimal PricePerUnit,
    string CurrencyId,
    decimal? CurrencyConvertValue) : ICommand<PurchaseRecordDto>;

internal sealed class UpdatePurchaseRecordCommandHandler(
    IInvestmentStrategyRepository investmentStrategyRepository,
    ICurrencyRepository currencyRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<UpdatePurchaseRecordCommand, PurchaseRecordDto>
{
    public async Task<Result<PurchaseRecordDto>> Handle(UpdatePurchaseRecordCommand request,
        CancellationToken cancellationToken)
    {
        Currency? currency = await currencyRepository.GetAsync(request.CurrencyId, cancellationToken);

        if (currency is null)
        {
            return Result.Failure<PurchaseRecordDto>(CurrencyErrors.NotFound(request.CurrencyId));
        }

        InvestmentStrategy? investmentStrategy =
            await investmentStrategyRepository.GetAsync(request.InvestmentStrategyId, cancellationToken);
        if (investmentStrategy is null)
        {
            return Result.Failure<PurchaseRecordDto>(InvestmentStrategyErrors.NotFound(request.InvestmentStrategyId));
        }

        Result<PurchaseRecord> result = investmentStrategy.UpdatePurchaseRecordOfGivenInvestment(
            request.InvestmentId, request.PurchaseRecordId, request.PurchaseDate, request.Amount, request.PricePerUnit,
            request.CurrencyId, request.CurrencyConvertValue, timeProvider.UtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new PurchaseRecordDto(
            result.Value.Id,
            result.Value.PurchaseDate,
            result.Value.Amount,
            result.Value.PricePerUnit,
            result.Value.TotalPrice,
            result.Value.CurrencyId,
            result.Value.InvestmentId,
            result.Value.CurrencyConvertValue);

        return dto;
    }
}

[UsedImplicitly]
internal sealed class UpdatePurchaseRecordCommandValidator : AbstractValidator<UpdatePurchaseRecordCommand>
{
    public UpdatePurchaseRecordCommandValidator()
    {
        RuleFor(x => x.PurchaseRecordId)
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

        RuleFor(x => x.CurrencyConvertValue)
            .GreaterThan(0)
            .When(x => x.CurrencyConvertValue.HasValue)
            .WithMessage("Currency convert value must be greater than zero when provided");
    }
}
