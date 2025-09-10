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

    public Investment AddInvestment(
        string name, 
        decimal modelPortfolioPercentage, 
        DateTime createdAtUtc)
    {
        // Check if adding this investment would exceed 100%
        decimal totalModelPercentage = _investments.Sum(i => i.ModelPortfolioPercentage) + modelPortfolioPercentage;
        if (totalModelPercentage > 100)
        {
            throw new InvalidOperationException($"Total model portfolio percentage cannot exceed 100%. Current: {_investments.Sum(i => i.ModelPortfolioPercentage)}, Adding: {modelPortfolioPercentage}");
        }

        var investment = Investment.Create(name, Id, modelPortfolioPercentage, createdAtUtc);
        _investments.Add(investment);
        UpdatedAtUtc = createdAtUtc;

        RecalculateRealPortfolioPercentages(createdAtUtc);

        Raise(new InvestmentAddedToStrategyDomainEvent(Id, createdAtUtc));

        return investment;
    }

    public Result RemoveInvestment(string investmentId, DateTime updatedAtUtc)
    {
        Investment? investment = _investments.Find(i => i.Id == investmentId);
        if (investment == null)
        {
            return Result.Failure(InvestmentStrategyErrors.InvestmentNotFound(investmentId));
        }

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
        // Validate that all investments exist
        var existingIds = _investments.Select(i => i.Id).ToHashSet();
        var missingIds = investmentPercentages.Keys.Except(existingIds).ToList();
        if (missingIds.Count != 0)
        {
            return Result.Failure(InvestmentStrategyErrors.InvestmentNotFound(missingIds[0]));
        }

        // Validate total percentage
        decimal totalPercentage = investmentPercentages.Values.Sum();
        if (totalPercentage > 100)
        {
            return Result.Failure(InvestmentStrategyErrors.ModelPortfolioPercentageExceeds100(totalPercentage));
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
