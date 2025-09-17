using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategyTypes;

public sealed class InvestmentStrategyTypeCreatedDomainEvent(string strategyTypeId, DateTime createdAtUtc) : DomainEvent
{
    public string StrategyTypeId { get; init; } = strategyTypeId;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;
}
