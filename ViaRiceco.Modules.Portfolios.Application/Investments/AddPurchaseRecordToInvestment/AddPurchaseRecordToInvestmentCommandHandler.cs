using FluentValidation;
using JetBrains.Annotations;
using ViaRiceco.Common.Application.Abstractions;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Application.Abstractions.Data;
using ViaRiceco.Modules.Portfolios.Application.PurchaseRecords.Models;
using ViaRiceco.Modules.Portfolios.Domain.Currencies;
using ViaRiceco.Modules.Portfolios.Domain.Investments;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;
using ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;

namespace ViaRiceco.Modules.Portfolios.Application.Investments.AddPurchaseRecordToInvestment;

public sealed record AddPurchaseRecordToInvestmentCommand(
    string InvestmentStrategyId,
    string InvestmentId,
    DateTime PurchaseDate,
    decimal Amount,
    decimal PricePerUnit,
    string CurrencyId) : ICommand<PurchaseRecordDto>;

internal sealed class AddPurchaseRecordToInvestmentCommandHandler(
    IInvestmentRepository investmentRepository,
    IInvestmentStrategyRepository investmentStrategyRepository,
    ICurrencyRepository currencyRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : ICommandHandler<AddPurchaseRecordToInvestmentCommand, PurchaseRecordDto>
{
    public async Task<Result<PurchaseRecordDto>> Handle(AddPurchaseRecordToInvestmentCommand request, CancellationToken cancellationToken)
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

        Currency? currency = await currencyRepository.GetAsync(request.CurrencyId, cancellationToken);

        if (currency is null)
        {
            return Result.Failure<PurchaseRecordDto>(CurrencyErrors.NotFound(request.CurrencyId));
        }

        Result<PurchaseRecord> addResult = investment.AddPurchaseRecord(
            request.PurchaseDate,
            request.Amount,
            request.PricePerUnit,
            request.CurrencyId,
            investmentStrategy.UninvestedAmount,
            timeProvider.UtcNow());

        if (addResult.IsFailure)
        {
            return Result.Failure<PurchaseRecordDto>(addResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new PurchaseRecordDto(
            addResult.Value.Id,
            addResult.Value.PurchaseDate,
            addResult.Value.Amount,
            addResult.Value.PricePerUnit,
            addResult.Value.TotalPrice,
            addResult.Value.CurrencyId,
            addResult.Value.InvestmentId);

        return dto;
    }
}

[UsedImplicitly]
internal sealed class AddPurchaseRecordToInvestmentCommandValidator : AbstractValidator<AddPurchaseRecordToInvestmentCommand>
{
    public AddPurchaseRecordToInvestmentCommandValidator()
    {
        RuleFor(x => x.InvestmentId)
            .NotEmpty()
            .WithMessage("Investment ID is required");

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
