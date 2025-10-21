using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

public sealed class SettlementPeriodDeletedDomainEvent(string settlementPeriodId, int month, int year, DateTime deletedAtUtc) : DomainEvent
{
    public string SettlementPeriodId { get; init; } = settlementPeriodId;
    public int Month { get; init; } = month;
    public int Year { get; init; } = year;
    public DateTime DeletedAtUtc { get; init; } = deletedAtUtc;
}
