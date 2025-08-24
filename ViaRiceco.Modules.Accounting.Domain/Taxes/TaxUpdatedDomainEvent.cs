using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.Taxes;

public sealed class TaxUpdatedDomainEvent(string taxId, decimal value, string taxTypeId, DateTime updatedAtUtc) : DomainEvent
{
    public string TaxId { get; init; } = taxId;
    public decimal Value { get; init; } = value;
    public string TaxTypeId { get; init; } = taxTypeId;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;   
}
