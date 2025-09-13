using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.Investments;

public static class InvestmentErrors
{
    public static Error NotFound(string investmentId) => Error.NotFound(
        "Investment.NotFound", 
        $"Investment with Id '{investmentId}' was not found.");
    
    public static Error PurchaseRecordNotFound(string purchaseRecordId) => Error.NotFound(
        "Investment.PurchaseRecordNotFound", 
        $"Purchase record with Id '{purchaseRecordId}' was not found.");
}
