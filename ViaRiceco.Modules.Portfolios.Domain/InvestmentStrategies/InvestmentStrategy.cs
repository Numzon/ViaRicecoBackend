using System.Transactions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Domain.Investments;
using ViaRiceco.Modules.Portfolios.Domain.InvestedCashRecords;

namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

public sealed class InvestmentStrategy : Entity
{
    private readonly List<Investment> _investments = [];
    private readonly List<InvestedCashRecord> _investedCashRecords = [];

    private InvestmentStrategy()
    {
    }

    public string FinancialGoalId { get; private set; } = string.Empty; // Only root financial goals (parentId == null)
    public string InvestmentStrategyTypeId { get; private set; } = string.Empty; // Used for grouping and filtering
    public decimal UninvestedAmount { get; private set; } // Free amount that can be used to buy new assets

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

    public Result UpdateInvestmentsModelPercentages(
        Dictionary<string, decimal> investmentPercentages,
        DateTime updatedAtUtc)
    {
        if (!InvestmentStrategySpecification.AllInvestmentsExist(this, investmentPercentages.Keys))
        {
            var existingIds = _investments.Select(i => i.Id).ToHashSet();
            var missingIds = investmentPercentages.Keys.Except(existingIds).ToList();
            return Result.Failure(InvestmentStrategyErrors.InvestmentNotFound(missingIds[0]));
        }

        // Validate individual percentages are between 0-100%
        if (investmentPercentages.Values.Any(percentage => percentage < 0 || percentage > 100))
        {
            return Result.Failure(InvestmentStrategyErrors.ModelPortfolioPercentageMustBeBetween0And100());
        }

        // Business rule: Portfolio model percentages must sum to exactly 100%
        if (!InvestmentStrategySpecification.DoModelPortfolioPercentagesSumTo100(investmentPercentages))
        {
            return Result.Failure(InvestmentStrategyErrors.ModelPortfolioPercentagesMustSumTo100());
        }

        // Update investments
        foreach (KeyValuePair<string, decimal> kvp in investmentPercentages)
        {
            Investment investment = _investments.First(i => i.Id == kvp.Key);
            investment.Update(investment.Name, kvp.Value, updatedAtUtc);
        }

        UpdatedAtUtc = updatedAtUtc;
        RecalculateRealPortfolioPercentages(updatedAtUtc);

        Raise(new InvestmentStrategyModelPercentagesUpdatedDomainEvent(Id, investmentPercentages, updatedAtUtc));

        return Result.Success();
    }

    public void UpdateInvestmentCurrentAmounts(Dictionary<string, decimal> investmentCurrentAmounts,
        DateTime updatedAtUtc)
    {
        foreach (KeyValuePair<string, decimal> kvp in investmentCurrentAmounts)
        {
            Investment? investment = _investments.Find(i => i.Id == kvp.Key);
            investment?.UpdateCurrentAmount(kvp.Value, updatedAtUtc);
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

    public Result<IReadOnlyCollection<InvestedCashRecord>> UpdateInvestedCashRecords(
        IReadOnlyCollection<KeyValuePair<string, decimal>> investedCashRecords, DateTime now)
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

        Raise(new InvestmentStrategyBalanceUpdatedDomainEvent(Id, now));
        
        return Result.Success(InvestedCashRecords);
    }
    
    public void NotifyInvestedCashRecordsUpdated(DateTime updatedAtUtc)
    {
        UpdatedAtUtc = updatedAtUtc;
        Raise(new InvestedCashRecordsUpdatedDomainEvent(Id, updatedAtUtc));
    }
}
