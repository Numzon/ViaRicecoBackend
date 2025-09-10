using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;

public sealed class PurchaseRecordCreatedDomainEvent(string recordId, DateTime createdAtUtc) : DomainEvent
{
    public string RecordId { get; init; } = recordId;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;
}
