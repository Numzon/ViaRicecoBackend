namespace ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

public static class SettlementPeriodSpecification
{
    /// <summary>
    /// Determines whether the settlement period contains any financial data (incomes or taxes)
    /// </summary>
    public static bool HasFinancialData(SettlementPeriod settlementPeriod)
    {
        return settlementPeriod.Incomes.Count > 0 || settlementPeriod.Taxes.Count > 0;
    }

    /// <summary>
    /// Determines whether the settlement period already has a tax for the specified tax type
    /// </summary>
    public static bool HasTaxTypeAlready(SettlementPeriod settlementPeriod, string taxTypeId)
    {
        return settlementPeriod.Taxes.Any(t => t.TaxTypeId == taxTypeId);
    }
}
