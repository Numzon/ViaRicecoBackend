using System.Transactions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Domain.Investments;
using ViaRiceco.Modules.Portfolios.Domain.InvestedCashRecords;
using ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;

namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

public sealed class InvestmentStrategy : Entity
{
    private readonly List<Investment> _investments = [];
    private readonly List<InvestedCashRecord> _investedCashRecords = [];

    private InvestmentStrategy()
    {
    }

    public string FinancialGoalId { get; private set; } = string.Empty; 
    public string InvestmentStrategyTypeId { get; private set; } = string.Empty; 
    public decimal UninvestedAmount { get; private set; } 

    public IReadOnlyCollection<Investment> Investments => _investments.AsReadOnly();
    public IReadOnlyCollection<InvestedCashRecord> InvestedCashRecords => _investedCashRecords.AsReadOnly();

    // Calculated properties
    public decimal TotalInvestedAmount => _investments.Sum(i => i.InvestedAmount);
    public decimal TotalCurrentAmount => _investments.Sum(i => i.CurrentAmount);
    public decimal TotalInvestedCash => _investedCashRecords.Sum(h => h.Amount);
    public decimal TotalAmount => TotalCurrentAmount + UninvestedAmount;

    public static InvestmentStrategy Create(
        string financialGoalId,
        string investmentStrategyTypeId,
        decimal uninvestedAmount,
        DateTime createdAtUtc)
    {
        var strategy = new InvestmentStrategy
        {
            Id = $"is_{Guid.NewGuid()}",
            FinancialGoalId = financialGoalId,
            InvestmentStrategyTypeId = investmentStrategyTypeId,
            UninvestedAmount = uninvestedAmount,
            CreatedAtUtc = createdAtUtc
        };

        strategy.Raise(new InvestmentStrategyCreatedDomainEvent(strategy.Id, createdAtUtc));

        return strategy;
    }

    public void UpdateUninvestedAmount(DateTime updatedAtUtc)
    {
        decimal uninvestedAmount = Math.Max(TotalInvestedCash - TotalInvestedAmount, 0m);

        if (UninvestedAmount == uninvestedAmount)
        {
            return;
        }

        UninvestedAmount = uninvestedAmount;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new InvestmentStrategyUninvestedAmountUpdatedDomainEvent(Id, UninvestedAmount, updatedAtUtc));
    }

    public Result<Investment> AddInvestment(
        string name,
        DateTime createdAtUtc)
    {
        var investment = Investment.Create(name, Id, createdAtUtc);
        _investments.Add(investment);
        UpdatedAtUtc = createdAtUtc;

        RecalculateRealPortfolioPercentages(createdAtUtc);

        Raise(new InvestmentAddedToStrategyDomainEvent(Id, createdAtUtc));

        return Result.Success(investment);
    }

    public Result RemoveInvestment(string investmentId, DateTime updatedAtUtc)
    {
        if (!InvestmentStrategySpecification.InvestmentExists(this, investmentId))
        {
            return Result.Failure(InvestmentStrategyErrors.InvestmentNotFound(investmentId));
        }

        Investment investment = _investments.First(i => i.Id == investmentId);
        _investments.Remove(investment);
        UpdatedAtUtc = updatedAtUtc;

        RecalculateRealPortfolioPercentages(updatedAtUtc);

        Raise(new InvestmentRemovedFromStrategyDomainEvent(Id, updatedAtUtc));

        return Result.Success();
    }

    public Result UpdateInvestmentsModelPercentages(IDictionary<string, decimal> investmentPercentages,
        DateTime updatedAtUtc)
    {
        var percentagesList = investmentPercentages.ToList();
        var investmentIds = percentagesList.Select(p => p.Key).ToList();

        if (!InvestmentStrategySpecification.AllInvestmentsExist(this, investmentIds))
        {
            var existingIds = _investments.Select(i => i.Id).ToHashSet();
            var missingIds = investmentIds.Except(existingIds).ToList();
            return Result.Failure(InvestmentStrategyErrors.InvestmentNotFound(missingIds[0]));
        }

        if (percentagesList.Any(p => p.Value < 0 || p.Value > 100))
        {
            return Result.Failure(InvestmentStrategyErrors.ModelPortfolioPercentageMustBeBetween0And100());
        }

        if (!InvestmentStrategySpecification.DoModelPortfolioPercentagesSumTo100(
                percentagesList.Select(p => p.Value)))
        {
            return Result.Failure(InvestmentStrategyErrors.ModelPortfolioPercentagesMustSumTo100());
        }

        foreach (KeyValuePair<string, decimal> investmentPercentage in percentagesList)
        {
            Investment investment = _investments.First(i => i.Id == investmentPercentage.Key);
            investment.Update(investment.Name, investmentPercentage.Value, updatedAtUtc);
        }

        UpdatedAtUtc = updatedAtUtc;
        RecalculateRealPortfolioPercentages(updatedAtUtc);

        RaiseInvestmentStrategyModelPercentagesUpdated(investmentPercentages, updatedAtUtc);

        return Result.Success();
    }

    private void RaiseInvestmentStrategyModelPercentagesUpdated(IDictionary<string, decimal> investmentPercentages,
        DateTime updatedAtUtc)
    {
        var models = investmentPercentages
            .Select(x => new InvestmentPercentage(x.Key, x.Value))
            .ToList();
        
        Raise(new InvestmentStrategyModelPercentagesUpdatedDomainEvent(Id, models, updatedAtUtc));
    }

    public void UpdateInvestmentCurrentAmounts(IDictionary<string, decimal> investmentCurrentAmounts,
        DateTime updatedAtUtc)
    {
        foreach (KeyValuePair<string, decimal> investmentCurrentAmount in investmentCurrentAmounts)
        {
            Investment? investment = _investments.Find(i => i.Id == investmentCurrentAmount.Key);
            investment?.UpdateCurrentAmount(investmentCurrentAmount.Value, updatedAtUtc);
        }

        UpdatedAtUtc = updatedAtUtc;
        RecalculateRealPortfolioPercentages(updatedAtUtc);

        Raise(new InvestmentStrategyCurrentAmountsUpdatedDomainEvent(Id, TotalCurrentAmount, TotalAmount,
            updatedAtUtc));
    }

    private void RecalculateRealPortfolioPercentages(DateTime updatedAtUtc)
    {
        if (TotalCurrentAmount == 0)
        {
            foreach (Investment investment in _investments)
            {
                investment.UpdateRealPortfolioPercentage(0, updatedAtUtc);
            }

            return;
        }

        foreach (Investment investment in _investments)
        {
            decimal realPercentage = investment.CurrentAmount / TotalCurrentAmount * 100;
            investment.UpdateRealPortfolioPercentage(realPercentage, updatedAtUtc);
        }

        Raise(new InvestmentStrategyRealPercentagesRecalculatedDomainEvent(Id, TotalCurrentAmount, updatedAtUtc));
    }

    public void UpdateInvestedCashRecords(
        IDictionary<string, decimal> investedCashRecords, DateTime now)
    {
        foreach (KeyValuePair<string, decimal> record in investedCashRecords)
        {
            InvestedCashRecord? existingRecord =
                _investedCashRecords.Find(i => i.MonthlyBudgetExpenseId == record.Key);

            if (existingRecord != null)
            {
                existingRecord.UpdateAmount(record.Value, now);
            }
            else
            {
                var newRecord = InvestedCashRecord.Create(
                    Id,
                    record.Key,
                    record.Value,
                    now);

                _investedCashRecords.Add(newRecord);
            }
        }

        UpdateUninvestedAmount(now);

        Raise(new InvestmentStrategyBalanceUpdatedDomainEvent(Id, now));
    }

    public Result RemoveInvestedCashRecord(string monthlyBudgetExpenseId, DateTime now)
    {
        InvestedCashRecord? investedCashRecord =
            _investedCashRecords.Find(i => i.MonthlyBudgetExpenseId == monthlyBudgetExpenseId);

        if (investedCashRecord == null)
        {
            return Result.Success();
        }

        _investedCashRecords.Remove(investedCashRecord);
        UpdateUninvestedAmount(now);

        Raise(new InvestmentStrategyBalanceUpdatedDomainEvent(Id, now));

        return Result.Success();
    }

    public Result RemovePurchaseRecordFromInvestment(string investmentId, string purchaseRecordId, DateTime now)
    {
        Investment? investment = _investments.Find(i => i.Id == investmentId);
        if (investment == null)
        {
            return Result.Failure(InvestmentStrategyErrors.InvestmentNotFound(investmentId));
        }

        Result result = investment.RemovePurchaseRecord(purchaseRecordId, now);

        if (result.IsFailure)
        {
            return result;
        }

        UpdateUninvestedAmount(now);

        Raise(new InvestmentStrategyBalanceUpdatedDomainEvent(Id, now));

        return Result.Success();
    }

    public Result<PurchaseRecord> AddPurchaseRecordToInvestment(
        string investmentId,
        DateTime purchaseDate,
        decimal amount,
        decimal pricePerUnit,
        string currencyId,
        decimal? currencyConvertValue,
        DateTime now)
    {
        Investment? investment = _investments.Find(i => i.Id == investmentId);

        if (investment == null)
        {
            return Result.Failure<PurchaseRecord>(InvestmentStrategyErrors.InvestmentNotFound(investmentId));
        }

        Result<PurchaseRecord> result =
            investment.AddPurchaseRecord(purchaseDate, amount, pricePerUnit, currencyId, UninvestedAmount,
                currencyConvertValue, now);

        if (result.IsFailure)
        {
            return result;
        }

        UpdateUninvestedAmount(now);

        Raise(new InvestmentStrategyBalanceUpdatedDomainEvent(Id, now));

        return Result.Success(result.Value);
    }

    public Result<PurchaseRecord> UpdatePurchaseRecordOfGivenInvestment(
        string investmentId,
        string purchaseRecordId,
        DateTime purchaseDate,
        decimal amount,
        decimal pricePerUnit,
        string currencyId,
        decimal? currencyConvertValue,
        DateTime now)
    {
        Investment? investment = _investments.Find(i => i.Id == investmentId);

        if (investment == null)
        {
            return Result.Failure<PurchaseRecord>(InvestmentStrategyErrors.InvestmentNotFound(investmentId));
        }

        Result<PurchaseRecord> result =
            investment.UpdatePurchaseRecord(purchaseRecordId, purchaseDate, amount, pricePerUnit, currencyId,
                UninvestedAmount, currencyConvertValue, now);

        if (result.IsFailure)
        {
            return result;
        }

        UpdateUninvestedAmount(now);

        Raise(new InvestmentStrategyBalanceUpdatedDomainEvent(Id, now));

        return Result.Success(result.Value);
    }
}
