using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

public sealed class InvestmentStrategyRealPercentagesRecalculatedDomainEvent(
    string strategyId, 
    decimal totalCurrentAmount, 
    DateTime updatedAtUtc) : DomainEvent
{
    public string StrategyId { get; init; } = strategyId;
    public decimal TotalCurrentAmount { get; init; } = totalCurrentAmount;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;
}
