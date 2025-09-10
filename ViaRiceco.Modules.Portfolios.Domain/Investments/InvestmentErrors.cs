using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.Investments;

public static class InvestmentErrors
{
    public static Error PurchaseRecordNotFound(string purchaseRecordId) => Error.NotFound(
        "Investment.PurchaseRecordNotFound", 
        $"Purchase record with Id '{purchaseRecordId}' was not found.");
}
