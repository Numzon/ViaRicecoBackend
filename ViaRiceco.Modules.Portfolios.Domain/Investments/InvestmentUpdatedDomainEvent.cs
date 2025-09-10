using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.Investments;

public sealed class InvestmentUpdatedDomainEvent(
    string investmentId, 
    string name, 
    decimal modelPortfolioPercentage, 
    DateTime updatedAtUtc) : DomainEvent
{
    public string InvestmentId { get; init; } = investmentId;
    public string Name { get; init; } = name;
    public decimal ModelPortfolioPercentage { get; init; } = modelPortfolioPercentage;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;
}
