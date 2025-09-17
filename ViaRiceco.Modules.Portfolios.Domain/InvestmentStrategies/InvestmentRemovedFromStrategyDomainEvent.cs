using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

public sealed class InvestmentRemovedFromStrategyDomainEvent(string strategyId, DateTime updatedAtUtc) : DomainEvent
{
    public string StrategyId { get; init; } = strategyId;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;
}
