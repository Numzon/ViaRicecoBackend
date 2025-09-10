using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Domain.Investments;

namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

public sealed class InvestmentStrategy : Entity
{
    private readonly List<Investment> _investments = [];

    private InvestmentStrategy()
    {
    }

    public string FinancialGoalId { get; private set; } = string.Empty; // Only root financial goals (parentId == null)
    public string InvestmentStrategyTypeId { get; private set; } = string.Empty; // Used for grouping and filtering
    public decimal UninvestedAmount { get; private set; } // Free amount that can be used to buy new assets

    public IReadOnlyCollection<Investment> Investments => _investments.AsReadOnly();
    
    // Calculated properties
    public decimal TotalInvestedAmount => _investments.Sum(i => i.InvestedAmount);
    public decimal TotalCurrentAmount => _investments.Sum(i => i.CurrentAmount);
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

    public void UpdateUninvestedAmount(decimal uninvestedAmount, DateTime updatedAtUtc)
    {
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
        decimal modelPortfolioPercentage, 
        DateTime createdAtUtc)
    {
        if (InvestmentStrategySpecification.WouldModelPortfolioPercentageExceed100(this, modelPortfolioPercentage))
        {
            return Result.Failure<Investment>(InvestmentStrategyErrors.ModelPortfolioPercentageExceeds100());
        }

        var investment = Investment.Create(name, Id, modelPortfolioPercentage, createdAtUtc);
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

        if (InvestmentStrategySpecification.WouldModelPortfolioPercentagesExceed100(investmentPercentages))
        {
            return Result.Failure(InvestmentStrategyErrors.ModelPortfolioPercentageExceeds100());
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

    public void UpdateInvestmentCurrentAmounts(Dictionary<string, decimal> investmentCurrentAmounts, DateTime updatedAtUtc)
    {
        foreach (KeyValuePair<string, decimal> kvp in investmentCurrentAmounts)
        {
            Investment? investment = _investments.Find(i => i.Id == kvp.Key);
            investment?.UpdateCurrentAmount(kvp.Value, updatedAtUtc);
        }

        UpdatedAtUtc = updatedAtUtc;
        RecalculateRealPortfolioPercentages(updatedAtUtc);

        Raise(new InvestmentStrategyCurrentAmountsUpdatedDomainEvent(Id, TotalCurrentAmount, TotalAmount, updatedAtUtc));
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

        // Calculate real percentages based on current amounts
        foreach (Investment investment in _investments)
        {
            decimal realPercentage = investment.CurrentAmount / TotalCurrentAmount * 100;
            investment.UpdateRealPortfolioPercentage(realPercentage, updatedAtUtc);
        }

        Raise(new InvestmentStrategyRealPercentagesRecalculatedDomainEvent(Id, TotalCurrentAmount, updatedAtUtc));
    }
}
