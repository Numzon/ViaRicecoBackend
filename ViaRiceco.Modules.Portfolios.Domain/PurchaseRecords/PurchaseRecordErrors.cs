using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;

public static class PurchaseRecordErrors
{
    public static Error NotFound(string recordId) => Error.NotFound(
        "PurchaseRecord.NotFound", 
        $"Purchase record with Id '{recordId}' was not found.");
    
    public static Error InvalidAmount(decimal amount) => Error.Validation(
        "PurchaseRecord.InvalidAmount", 
        $"Purchase amount '{amount}' is invalid. Amount must be greater than zero.");
    
    public static Error InvalidPricePerUnit(decimal pricePerUnit) => Error.Validation(
        "PurchaseRecord.InvalidPricePerUnit", 
        $"Price per unit '{pricePerUnit}' is invalid. Price must be greater than zero.");
    
    public static Error FuturePurchaseDate(DateTime purchaseDate) => Error.Validation(
        "PurchaseRecord.FuturePurchaseDate", 
        $"Purchase date '{purchaseDate:yyyy-MM-dd}' cannot be in the future.");
    
    public static Error InvalidCurrency(string currencyId) => Error.Validation(
        "PurchaseRecord.InvalidCurrency", 
        $"Currency Id '{currencyId}' is required and cannot be empty.");
    
    public static Error InvalidInvestment(string investmentId) => Error.Validation(
        "PurchaseRecord.InvalidInvestment", 
        $"Investment Id '{investmentId}' is required and cannot be empty.");

    public static Error InvalidSortParameter(string? sort) => Error.Validation(
        "PurchaseRecord.InvalidSortParameter", 
        $"Invalid sort parameter: '{sort}'");
}
