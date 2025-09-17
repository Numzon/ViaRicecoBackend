using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

public sealed class InvestmentStrategyUninvestedAmountUpdatedDomainEvent(
    string strategyId, 
    decimal uninvestedAmount, 
    DateTime updatedAtUtc) : DomainEvent
{
    public string StrategyId { get; init; } = strategyId;
    public decimal UninvestedAmount { get; init; } = uninvestedAmount;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;
}
