using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

public sealed class InvestmentStrategyCurrentAmountsUpdatedDomainEvent(
    string strategyId, 
    decimal totalCurrentAmount, 
    decimal totalAmount, 
    DateTime updatedAtUtc) : DomainEvent
{
    public string StrategyId { get; init; } = strategyId;
    public decimal TotalCurrentAmount { get; init; } = totalCurrentAmount;
    public decimal TotalAmount { get; init; } = totalAmount;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;
}
