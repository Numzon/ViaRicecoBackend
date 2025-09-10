using ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;

namespace ViaRiceco.Modules.Portfolios.Domain.Investments;

public static class InvestmentSpecification
{
    /// <summary>
    /// Determines whether the specified purchase record exists in the investment
    /// </summary>
    public static bool PurchaseRecordExists(Investment investment, string purchaseRecordId)
    {
        return investment.PurchaseRecords.Any(pr => pr.Id == purchaseRecordId);
    }

    /// <summary>
    /// Determines whether the investment has any purchase records
    /// </summary>
    public static bool HasPurchaseRecords(Investment investment)
    {
        return investment.PurchaseRecords.Count > 0;
    }

    /// <summary>
    /// Determines whether the investment name is valid (not null or empty)
    /// </summary>
    public static bool IsValidName(string? name)
    {
        return !string.IsNullOrWhiteSpace(name);
    }

    /// <summary>
    /// Determines whether the model portfolio percentage is within valid range (0-100)
    /// </summary>
    public static bool IsValidModelPortfolioPercentage(decimal percentage)
    {
        return percentage >= 0 && percentage <= 100;
    }

    /// <summary>
    /// Determines whether the investment strategy ID is valid (not null or empty)
    /// </summary>
    public static bool IsValidInvestmentStrategyId(string? investmentStrategyId)
    {
        return !string.IsNullOrWhiteSpace(investmentStrategyId);
    }

    /// <summary>
    /// Determines whether all investment creation parameters are valid
    /// </summary>
    public static bool AreCreateParametersValid(
        string? name,
        string? investmentStrategyId,
        decimal modelPortfolioPercentage)
    {
        return IsValidName(name) &&
               IsValidInvestmentStrategyId(investmentStrategyId) &&
               IsValidModelPortfolioPercentage(modelPortfolioPercentage);
    }
}
