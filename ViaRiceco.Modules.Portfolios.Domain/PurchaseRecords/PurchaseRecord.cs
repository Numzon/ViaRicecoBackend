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

    public static Result<PurchaseRecord> Create(
        DateTime purchaseDate, 
        decimal amount, 
        decimal pricePerUnit, 
        string currencyId, 
        string investmentId,
        DateTime createdAtUtc)
    {
        Result validationResult = ValidateCreateParameters(purchaseDate, amount, pricePerUnit, currencyId, investmentId);
        if (validationResult.IsFailure)
        {
            return Result.Failure<PurchaseRecord>(validationResult.Error);
        }

        // Calculate total price to ensure consistency
        decimal totalPrice = amount * pricePerUnit;
        
        var record = new PurchaseRecord
        {
            Id = $"pr_{Guid.NewGuid()}",
            PurchaseDate = purchaseDate,
            Amount = amount,
            PricePerUnit = pricePerUnit,
            TotalPrice = totalPrice,
            CurrencyId = currencyId,
            InvestmentId = investmentId,
            CreatedAtUtc = createdAtUtc
        };

        record.Raise(new PurchaseRecordCreatedDomainEvent(record.Id, createdAtUtc));

        return Result.Success(record);
    }

    public Result Update(
        DateTime purchaseDate, 
        decimal amount, 
        decimal pricePerUnit, 
        string currencyId, 
        DateTime updatedAtUtc)
    {
        Result validationResult = ValidateUpdateParameters(purchaseDate, amount, pricePerUnit, currencyId);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        decimal totalPrice = amount * pricePerUnit;
        
        if (PurchaseDate == purchaseDate && 
            Amount == amount && 
            PricePerUnit == pricePerUnit && 
            CurrencyId == currencyId)
        {
            return Result.Success();
        }

        PurchaseDate = purchaseDate;
        Amount = amount;
        PricePerUnit = pricePerUnit;
        TotalPrice = totalPrice;
        CurrencyId = currencyId;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new PurchaseRecordUpdatedDomainEvent(
            Id, 
            PurchaseDate, 
            Amount, 
            PricePerUnit, 
            TotalPrice, 
            CurrencyId, 
            updatedAtUtc));
            
        return Result.Success();
    }

    private static Result ValidateCreateParameters(
        DateTime purchaseDate, 
        decimal amount, 
        decimal pricePerUnit, 
        string currencyId, 
        string investmentId)
    {
        if (amount <= 0)
        {
            return Result.Failure(PurchaseRecordErrors.InvalidAmount(amount));
        }
        
        if (pricePerUnit <= 0)
        {
            return Result.Failure(PurchaseRecordErrors.InvalidPricePerUnit(pricePerUnit));
        }
        
        if (purchaseDate > DateTime.UtcNow)
        {
            return Result.Failure(PurchaseRecordErrors.FuturePurchaseDate(purchaseDate));
        }
        
        if (string.IsNullOrWhiteSpace(currencyId))
        {
            return Result.Failure(PurchaseRecordErrors.InvalidCurrency(currencyId));
        }
        
        if (string.IsNullOrWhiteSpace(investmentId))
        {
            return Result.Failure(PurchaseRecordErrors.InvalidInvestment(investmentId));
        }

        return Result.Success();
    }

    private static Result ValidateUpdateParameters(
        DateTime purchaseDate, 
        decimal amount, 
        decimal pricePerUnit, 
        string currencyId)
    {
        if (amount <= 0)
        {
            return Result.Failure(PurchaseRecordErrors.InvalidAmount(amount));
        }
        
        if (pricePerUnit <= 0)
        {
            return Result.Failure(PurchaseRecordErrors.InvalidPricePerUnit(pricePerUnit));
        }
        
        if (purchaseDate > DateTime.UtcNow)
        {
            return Result.Failure(PurchaseRecordErrors.FuturePurchaseDate(purchaseDate));
        }
        
        if (string.IsNullOrWhiteSpace(currencyId))
        {
            return Result.Failure(PurchaseRecordErrors.InvalidCurrency(currencyId));
        }

        return Result.Success();
    }
}
