using System.Globalization;

namespace ViaRiceco.Modules.Portfolios.Domain.Currencies;

public static class CurrencySpecification
{
    /// <summary>
    /// Determines whether the currency name is valid (not null or empty)
    /// </summary>
    public static bool IsValidName(string? name)
    {
        return !string.IsNullOrWhiteSpace(name);
    }

    /// <summary>
    /// Determines whether the currency code is valid (exactly 3 letters)
    /// </summary>
    public static bool IsValidCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        return code.Trim().Length == 3 && code.Trim().All(char.IsLetter);
    }

    /// <summary>
    /// Determines whether the currency code should be normalized (converted to uppercase)
    /// </summary>
    public static bool ShouldNormalizeCode(string code)
    {
        return !string.Equals(code, code.ToUpper(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    /// <summary>
    /// Normalizes the currency code to uppercase using invariant culture
    /// </summary>
    public static string NormalizeCode(string code)
    {
        return code.Trim().ToUpper(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Determines whether all currency creation parameters are valid
    /// </summary>
    public static bool AreCreateParametersValid(string? name, string? code)
    {
        return IsValidName(name) && IsValidCode(code);
    }

    /// <summary>
    /// Determines whether all currency update parameters are valid
    /// </summary>
    public static bool AreUpdateParametersValid(string? name, string? code)
    {
        return IsValidName(name) && IsValidCode(code);
    }
}
