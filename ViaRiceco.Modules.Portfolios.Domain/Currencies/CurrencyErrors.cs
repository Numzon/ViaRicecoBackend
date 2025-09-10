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
    
    public static Error InvalidCode(string code) => Error.Validation(
        "Currency.InvalidCode", 
        $"Currency code '{code}' is invalid. Currency codes must be 3 characters long and contain only letters.");
}
