using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.Investments;

public sealed class PurchaseRecordAddedToInvestmentDomainEvent(string investmentId, DateTime createdAtUtc) : DomainEvent
{
    public string InvestmentId { get; init; } = investmentId;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;
}
