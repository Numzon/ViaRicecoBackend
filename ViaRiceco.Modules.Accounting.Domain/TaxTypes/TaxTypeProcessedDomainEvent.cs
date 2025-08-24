using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.TaxTypes;

public sealed class TaxTypeProcessedDomainEvent(string taxTypeId, DateTime createdAtUtc) : DomainEvent
{
    public string TaxTypeId { get; init; } = taxTypeId;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;  
}
