using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.Currencies;

public static class CurrencyErrors
{
    public static Error NotFound(string currencyId) => Error.NotFound(
        "Currency.NotFound", 
        $"Currency with Id '{currencyId}' was not found.");
    
    public static Error DuplicateCode(string code) => Error.Conflict(
        "Currency.DuplicateCode", 
        $"Currency with code '{code}' already exists.");
    
    public static Error InvalidName() => Error.Validation(
        "Currency.InvalidName",
        "Currency name cannot be empty.");

    public static Error InvalidCode() => Error.Validation(
        "Currency.InvalidCode", 
        "Currency code must be exactly 3 letters.");
}
