using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.Investments;

public sealed class InvestmentRealPortfolioPercentageUpdatedDomainEvent(
    string investmentId, 
    decimal realPortfolioPercentage, 
    DateTime updatedAtUtc) : DomainEvent
{
    public string InvestmentId { get; init; } = investmentId;
    public decimal RealPortfolioPercentage { get; init; } = realPortfolioPercentage;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;
}
