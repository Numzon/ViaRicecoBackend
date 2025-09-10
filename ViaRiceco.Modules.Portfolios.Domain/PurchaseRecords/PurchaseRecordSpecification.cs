namespace ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;

public static class PurchaseRecordSpecification
{
    /// <summary>
    /// Determines whether the purchase date is valid (not in the future)
    /// </summary>
    public static bool IsValidPurchaseDate(DateTime purchaseDate)
    {
        return purchaseDate <= DateTime.UtcNow;
    }

    /// <summary>
    /// Determines whether the purchase amount is valid (greater than zero)
    /// </summary>
    public static bool IsValidAmount(decimal amount)
    {
        return amount > 0;
    }

    /// <summary>
    /// Determines whether the price per unit is valid (greater than zero)
    /// </summary>
    public static bool IsValidPricePerUnit(decimal pricePerUnit)
    {
        return pricePerUnit > 0;
    }

    /// <summary>
    /// Determines whether the currency ID is valid (not null or empty)
    /// </summary>
    public static bool IsValidCurrencyId(string? currencyId)
    {
        return !string.IsNullOrWhiteSpace(currencyId);
    }

    /// <summary>
    /// Determines whether the investment ID is valid (not null or empty)
    /// </summary>
    public static bool IsValidInvestmentId(string? investmentId)
    {
        return !string.IsNullOrWhiteSpace(investmentId);
    }

    /// <summary>
    /// Determines whether all purchase record parameters are valid for creation
    /// </summary>
    public static bool AreCreateParametersValid(
        DateTime purchaseDate,
        decimal amount,
        decimal pricePerUnit,
        string? currencyId,
        string? investmentId)
    {
        return IsValidPurchaseDate(purchaseDate) &&
               IsValidAmount(amount) &&
               IsValidPricePerUnit(pricePerUnit) &&
               IsValidCurrencyId(currencyId) &&
               IsValidInvestmentId(investmentId);
    }

    /// <summary>
    /// Determines whether all purchase record parameters are valid for update
    /// </summary>
    public static bool AreUpdateParametersValid(
        DateTime purchaseDate,
        decimal amount,
        decimal pricePerUnit,
        string? currencyId)
    {
        return IsValidPurchaseDate(purchaseDate) &&
               IsValidAmount(amount) &&
               IsValidPricePerUnit(pricePerUnit) &&
               IsValidCurrencyId(currencyId);
    }
}
