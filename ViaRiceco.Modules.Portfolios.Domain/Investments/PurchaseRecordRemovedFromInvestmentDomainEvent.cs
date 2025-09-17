using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.Investments;

public sealed class PurchaseRecordRemovedFromInvestmentDomainEvent(string investmentId, DateTime updatedAtUtc) : DomainEvent
{
    public string InvestmentId { get; init; } = investmentId;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;
}
