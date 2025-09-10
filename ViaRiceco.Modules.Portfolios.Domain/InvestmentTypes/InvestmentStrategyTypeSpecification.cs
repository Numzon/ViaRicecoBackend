namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentTypes;

public static class InvestmentStrategyTypeSpecification
{
    /// <summary>
    /// Determines whether the investment strategy type name is valid (not null or empty)
    /// </summary>
    public static bool IsValidName(string? name)
    {
        return !string.IsNullOrWhiteSpace(name);
    }

    /// <summary>
    /// Determines whether all investment strategy type creation parameters are valid
    /// </summary>
    public static bool AreCreateParametersValid(string? name)
    {
        return IsValidName(name);
    }

    /// <summary>
    /// Determines whether all investment strategy type update parameters are valid
    /// </summary>
    public static bool AreUpdateParametersValid(string? name)
    {
        return IsValidName(name);
    }
}
