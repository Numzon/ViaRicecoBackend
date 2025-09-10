using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentTypes;

public sealed class InvestmentStrategyTypeUpdatedDomainEvent(string strategyTypeId, string name, DateTime updatedAtUtc) : DomainEvent
{
    public string StrategyTypeId { get; init; } = strategyTypeId;
    public string Name { get; init; } = name;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;
}
