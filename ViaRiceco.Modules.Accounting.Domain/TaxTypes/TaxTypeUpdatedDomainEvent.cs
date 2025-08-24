using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.TaxTypes;

public sealed class TaxTypeUpdatedDomainEvent(string taxTypeId, string name, DateTime updatedAtUtc) : DomainEvent
{
    public string TaxTypeId { get; init; } = taxTypeId;
    public string Name { get; init; } = name;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;   
}
