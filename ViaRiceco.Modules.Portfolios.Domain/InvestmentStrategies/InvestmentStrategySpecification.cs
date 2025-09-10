namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

public static class InvestmentStrategySpecification
{
    /// <summary>
    /// Determines whether adding the specified model portfolio percentage would exceed 100%
    /// </summary>
    public static bool WouldModelPortfolioPercentageExceed100(
        InvestmentStrategy investmentStrategy, 
        decimal additionalPercentage)
    {
        decimal currentTotalPercentage = investmentStrategy.Investments.Sum(i => i.ModelPortfolioPercentage);
        return currentTotalPercentage + additionalPercentage > 100;
    }

    /// <summary>
    /// Determines whether the total of the specified model portfolio percentages would exceed 100%
    /// </summary>
    public static bool WouldModelPortfolioPercentagesExceed100(Dictionary<string, decimal> investmentPercentages)
    {
        return investmentPercentages.Values.Sum() > 100;
    }

    /// <summary>
    /// Determines whether all specified investment IDs exist in the strategy
    /// </summary>
    public static bool AllInvestmentsExist(InvestmentStrategy investmentStrategy, IEnumerable<string> investmentIds)
    {
        var existingIds = investmentStrategy.Investments.Select(i => i.Id).ToHashSet();
        return investmentIds.All(id => existingIds.Contains(id));
    }

    /// <summary>
    /// Determines whether the specified investment ID exists in the strategy
    /// </summary>
    public static bool InvestmentExists(InvestmentStrategy investmentStrategy, string investmentId)
    {
        return investmentStrategy.Investments.Any(i => i.Id == investmentId);
    }

    /// <summary>
    /// Determines whether the investment strategy has any investments
    /// </summary>
    public static bool HasInvestments(InvestmentStrategy investmentStrategy)
    {
        return investmentStrategy.Investments.Count > 0;
    }

    /// <summary>
    /// Determines whether the investment strategy has reached its maximum model portfolio allocation (100%)
    /// </summary>
    public static bool HasFullModelPortfolioAllocation(InvestmentStrategy investmentStrategy)
    {
        return investmentStrategy.Investments.Sum(i => i.ModelPortfolioPercentage) >= 100;
    }
}
