using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

public sealed class InvestmentStrategyCreatedDomainEvent(string investmentStrategyId, DateTime createdAtUtc) : DomainEvent
{
    public string InvestmentStrategyId { get; init; } = investmentStrategyId;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;
}
