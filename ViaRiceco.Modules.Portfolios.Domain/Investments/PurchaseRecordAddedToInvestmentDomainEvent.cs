using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.Investments;

public sealed class PurchaseRecordAddedToInvestmentDomainEvent(
    string investmentId,
    string investmentStrategyId,
    decimal totalAmount,
    DateTime createdAtUtc) : DomainEvent
{
    public string InvestmentId { get; init; } = investmentId;
    public string InvestmentStrategyId { get; init; } = investmentStrategyId;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;
    public decimal TotalAmount { get; init; } = totalAmount;
}
