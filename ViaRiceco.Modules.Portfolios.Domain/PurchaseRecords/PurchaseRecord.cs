using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;

public sealed class PurchaseRecord : Entity
{
    private PurchaseRecord()
    {
    }

    public DateTime PurchaseDate { get; private set; }
    public decimal Amount { get; private set; }
    public decimal PricePerUnit { get; private set; }
    public decimal TotalPrice { get; private set; }
    public string CurrencyId { get; private set; } = string.Empty;
    public string InvestmentId { get; private set; } = string.Empty;
    public decimal? CurrencyConvertValue { get; private set; }

    public static Result<PurchaseRecord> Create(
        DateTime purchaseDate,
        decimal amount,
        decimal pricePerUnit,
        string currencyId,
        string investmentId,
        decimal uninvestedAmount,
        decimal? currencyConvertValue,
        DateTime createdAtUtc)
    {
        decimal totalPrice = CalculateTotalPrice(amount, pricePerUnit, currencyConvertValue);

        Result validationResult = ValidateCreateParameters(purchaseDate, amount, pricePerUnit, currencyId, investmentId,
            totalPrice, uninvestedAmount, currencyConvertValue);
        if (validationResult.IsFailure)
        {
            return Result.Failure<PurchaseRecord>(validationResult.Error);
        }

        var record = new PurchaseRecord
        {
            Id = $"pr_{Guid.NewGuid()}",
            PurchaseDate = purchaseDate,
            Amount = amount,
            PricePerUnit = pricePerUnit,
            TotalPrice = totalPrice,
            CurrencyId = currencyId,
            InvestmentId = investmentId,
            CurrencyConvertValue = currencyConvertValue,
            CreatedAtUtc = createdAtUtc
        };

        record.Raise(new PurchaseRecordCreatedDomainEvent(record.Id, createdAtUtc));

        return Result.Success(record);
    }

    public Result<PurchaseRecord> Update(
        DateTime purchaseDate,
        decimal amount,
        decimal pricePerUnit,
        string currencyId,
        decimal uninvestedAmount,
        decimal? currencyConvertValue,
        DateTime updatedAtUtc)
    {
        Result validationResult =
            ValidateUpdateParameters(purchaseDate, amount, pricePerUnit, uninvestedAmount, currencyId, currencyConvertValue);
        if (validationResult.IsFailure)
        {
            return Result.Failure<PurchaseRecord>(validationResult.Error);
        }

        decimal totalPrice = CalculateTotalPrice(amount, pricePerUnit, currencyConvertValue);

        if (PurchaseDate == purchaseDate &&
            Amount == amount &&
            PricePerUnit == pricePerUnit &&
            CurrencyId == currencyId &&
            CurrencyConvertValue == currencyConvertValue)
        {
            return Result.Success(this);
        }

        PurchaseDate = purchaseDate;
        Amount = amount;
        PricePerUnit = pricePerUnit;
        TotalPrice = totalPrice;
        CurrencyId = currencyId;
        CurrencyConvertValue = currencyConvertValue;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new PurchaseRecordUpdatedDomainEvent(
            Id,
            PurchaseDate,
            Amount,
            PricePerUnit,
            TotalPrice,
            CurrencyId,
            CurrencyConvertValue,
            updatedAtUtc));

        return Result.Success(this);
    }

    private static Result ValidateCreateParameters(
        DateTime purchaseDate,
        decimal amount,
        decimal pricePerUnit,
        string currencyId,
        string investmentId,
        decimal totalPrice,
        decimal uninvestedAmount,
        decimal? currencyConvertValue)
    {
        if (!PurchaseRecordSpecification.IsValidAmount(amount))
        {
            return Result.Failure(PurchaseRecordErrors.InvalidAmount(amount));
        }

        if (!PurchaseRecordSpecification.IsValidPricePerUnit(pricePerUnit))
        {
            return Result.Failure(PurchaseRecordErrors.InvalidPricePerUnit(pricePerUnit));
        }

        if (!PurchaseRecordSpecification.IsValidPurchaseDate(purchaseDate))
        {
            return Result.Failure(PurchaseRecordErrors.FuturePurchaseDate(purchaseDate));
        }

        if (!PurchaseRecordSpecification.IsValidCurrencyId(currencyId))
        {
            return Result.Failure(PurchaseRecordErrors.InvalidCurrency(currencyId));
        }

        if (!PurchaseRecordSpecification.IsValidInvestmentId(investmentId))
        {
            return Result.Failure(PurchaseRecordErrors.InvalidInvestment(investmentId));
        }

        if (currencyConvertValue.HasValue && currencyConvertValue.Value <= 0)
        {
            return Result.Failure(PurchaseRecordErrors.InvalidCurrencyConvertValue(currencyConvertValue.Value));
        }

        // Business rule: Purchase cost cannot exceed available uninvested amount
        if (!PurchaseRecordSpecification.HasSufficientUninvestedAmount(totalPrice, uninvestedAmount))
        {
            return Result.Failure(PurchaseRecordErrors.InsufficientUninvestedAmount(totalPrice, uninvestedAmount));
        }

        return Result.Success();
    }

    private static Result ValidateUpdateParameters(
        DateTime purchaseDate,
        decimal amount,
        decimal pricePerUnit,
        decimal uninvestedAmount,
        string currencyId,
        decimal? currencyConvertValue)
    {
        if (!PurchaseRecordSpecification.IsValidAmount(amount))
        {
            return Result.Failure(PurchaseRecordErrors.InvalidAmount(amount));
        }

        if (!PurchaseRecordSpecification.IsValidPricePerUnit(pricePerUnit))
        {
            return Result.Failure(PurchaseRecordErrors.InvalidPricePerUnit(pricePerUnit));
        }

        if (!PurchaseRecordSpecification.IsValidPurchaseDate(purchaseDate))
        {
            return Result.Failure(PurchaseRecordErrors.FuturePurchaseDate(purchaseDate));
        }

        if (!PurchaseRecordSpecification.IsValidCurrencyId(currencyId))
        {
            return Result.Failure(PurchaseRecordErrors.InvalidCurrency(currencyId));
        }

        if (currencyConvertValue.HasValue && currencyConvertValue.Value <= 0)
        {
            return Result.Failure(PurchaseRecordErrors.InvalidCurrencyConvertValue(currencyConvertValue.Value));
        }

        decimal totalPrice = CalculateTotalPrice(amount, pricePerUnit, currencyConvertValue);

        if (!PurchaseRecordSpecification.HasSufficientUninvestedAmount(totalPrice, uninvestedAmount))
        {
            return Result.Failure(PurchaseRecordErrors.InsufficientUninvestedAmount(totalPrice, uninvestedAmount));
        }

        return Result.Success();
    }

    private static decimal CalculateTotalPrice(decimal amount, decimal pricePerUnit, decimal? currencyConvertValue)
    {
        // When currency conversion is provided, multiply by the conversion rate
        return currencyConvertValue.HasValue
            ? amount * pricePerUnit * currencyConvertValue.Value
            : amount * pricePerUnit;
    }
}
