using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;

public sealed class SettlementPeriodSetAsDraftDomainEvent(string settlementPeriodId, DateTime setAsDraftAtUtc) : DomainEvent
{
    public string SettlementPeriodId { get; init; } = settlementPeriodId;
    public DateTime SetAsDraftAtUtc { get; init; } = setAsDraftAtUtc;
}
