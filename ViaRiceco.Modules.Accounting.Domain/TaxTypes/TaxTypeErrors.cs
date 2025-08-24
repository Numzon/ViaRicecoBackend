using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.TaxTypes;

public static class TaxTypeErrors
{
    public static Error NotFound(string taxTypeId) =>
        Error.NotFound("TaxType.NotFound", $"The tax type with the identifier {taxTypeId} was not found");
}
