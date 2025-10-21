using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

public sealed class InvestmentPercentage(string investmentId, decimal percentage)
{
    public string InvestmentId { get; init; } = investmentId;
    public decimal Percentage { get; init; } = percentage;
}

public sealed class InvestmentStrategyModelPercentagesUpdatedDomainEvent(
    string strategyId, 
    List<InvestmentPercentage> investmentPercentages, 
    DateTime updatedAtUtc) : DomainEvent
{
    public string StrategyId { get; init; } = strategyId;
    public List<InvestmentPercentage> InvestmentPercentages { get; init; } = investmentPercentages;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;
}
