using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

public sealed class InvestmentStrategyCreatedDomainEvent(string strategyId, DateTime createdAtUtc) : DomainEvent
{
    public string StrategyId { get; init; } = strategyId;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;
}
