using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

public sealed class InvestmentStrategyModelPercentagesUpdatedDomainEvent(
    string strategyId, 
    Dictionary<string, decimal> investmentPercentages, 
    DateTime updatedAtUtc) : DomainEvent
{
    public string StrategyId { get; init; } = strategyId;
    public Dictionary<string, decimal> InvestmentPercentages { get; init; } = investmentPercentages;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;
}
