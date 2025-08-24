using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.Taxes;

public static class TaxErrors
{
    public static Error NotFound(string taxId) =>
        Error.NotFound("Tax.NotFound", $"The tax with the identifier {taxId} was not found");

    public static Error InvalidTaxType(string taxTypeId) =>
        Error.Conflict("Tax.InvalidTaxType", $"The tax type with identifier {taxTypeId} does not exist");

    public static Error SettlementPeriodNotFound(string settlementPeriodId) =>
        Error.NotFound("Tax.SettlementPeriodNotFound", $"The settlement period with identifier {settlementPeriodId} does not exist");
}
