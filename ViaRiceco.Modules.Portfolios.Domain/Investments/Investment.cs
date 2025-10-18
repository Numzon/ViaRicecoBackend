using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;

namespace ViaRiceco.Modules.Portfolios.Domain.Investments;

public sealed class Investment : Entity
{
    private readonly List<PurchaseRecord> _purchaseRecords = [];

    private Investment()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string InvestmentStrategyId { get; private set; } = string.Empty;
    public decimal ModelPortfolioPercentage { get; private set; }
    public decimal CurrentAmount { get; private set; }
    public decimal RealPortfolioPercentage { get; private set; }

    public IReadOnlyCollection<PurchaseRecord> PurchaseRecords => _purchaseRecords.AsReadOnly();
    
    public decimal InvestedAmount => _purchaseRecords.Sum(pr => pr.TotalPrice);
    
    public decimal CurrentInvestedDifference => CurrentAmount - InvestedAmount;

    public static Investment Create(
        string name,
        string investmentStrategyId,
        DateTime createdAtUtc)
    {
        var investment = new Investment
        {
            Id = $"i_{Guid.NewGuid()}",
            Name = name,
            InvestmentStrategyId = investmentStrategyId,
            ModelPortfolioPercentage = 0, // Default to 0%, set later via UpdateInvestmentsModelPercentages
            CurrentAmount = 0,
            RealPortfolioPercentage = 0,
            CreatedAtUtc = createdAtUtc
        };

        investment.Raise(new InvestmentCreatedDomainEvent(investment.Id, createdAtUtc));

        return investment;
    }

    public void Update(string name, decimal modelPortfolioPercentage, DateTime updatedAtUtc)
    {
        if (Name == name && ModelPortfolioPercentage == modelPortfolioPercentage)
        {
            return;
        }

        Name = name;
        ModelPortfolioPercentage = modelPortfolioPercentage;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new InvestmentUpdatedDomainEvent(Id, Name, ModelPortfolioPercentage, updatedAtUtc));
    }

    public void UpdateCurrentAmount(decimal currentAmount, DateTime updatedAtUtc)
    {
        if (CurrentAmount == currentAmount)
        {
            return;
        }

        CurrentAmount = currentAmount;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new InvestmentCurrentAmountUpdatedDomainEvent(Id, CurrentAmount, CurrentInvestedDifference,
            updatedAtUtc));
    }

    public void UpdateRealPortfolioPercentage(decimal realPortfolioPercentage, DateTime updatedAtUtc)
    {
        if (RealPortfolioPercentage == realPortfolioPercentage)
        {
            return;
        }

        RealPortfolioPercentage = realPortfolioPercentage;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new InvestmentRealPortfolioPercentageUpdatedDomainEvent(Id, RealPortfolioPercentage, updatedAtUtc));
    }

    public Result<PurchaseRecord> AddPurchaseRecord(
        DateTime purchaseDate,
        decimal amount,
        decimal pricePerUnit,
        string currencyId,
        decimal uninvestedAmount,
        DateTime createdAtUtc)
    {
        Result<PurchaseRecord> createResult = PurchaseRecord.Create(
            purchaseDate,
            amount,
            pricePerUnit,
            currencyId,
            Id,
            uninvestedAmount,
            createdAtUtc);

        if (createResult.IsFailure)
        {
            return createResult;
        }

        _purchaseRecords.Add(createResult.Value);
        UpdatedAtUtc = createdAtUtc;
        
        return createResult;
    }

    public Result RemovePurchaseRecord(string purchaseRecordId, DateTime updatedAtUtc)
    {
        if (!InvestmentSpecification.PurchaseRecordExists(this, purchaseRecordId))
        {
            return Result.Failure(PurchaseRecordErrors.NotFound(purchaseRecordId));
        }

        PurchaseRecord purchaseRecord = _purchaseRecords.First(pr => pr.Id == purchaseRecordId);
        _purchaseRecords.Remove(purchaseRecord);
        UpdatedAtUtc = updatedAtUtc;

        return Result.Success();
    }

    public Result<PurchaseRecord> UpdatePurchaseRecord(
        string purchaseRecordId,
        DateTime purchaseDate,
        decimal amount,
        decimal pricePerUnit,
        string currencyId,
        decimal uninvestedAmount,
        DateTime now)
    {
        PurchaseRecord? purchaseRecord = _purchaseRecords.Find(i => i.Id == purchaseRecordId);

        if (purchaseRecord == null)
        {
            return Result.Failure<PurchaseRecord>(InvestmentErrors.PurchaseRecordNotFound(purchaseRecordId));
        }
        
        Result<PurchaseRecord> result = purchaseRecord.Update(purchaseDate, amount, pricePerUnit, currencyId, uninvestedAmount, now);
        
        if (result.IsFailure)
        {
            return result;
        }
        
        UpdatedAtUtc = now;
        
        return result;
    }
}
