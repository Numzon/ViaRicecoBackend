using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

public sealed class InvestedCashRecordsUpdatedDomainEvent(string investmentStrategyId, DateTime occurredAtUtc) : DomainEvent
{
    public string InvestmentStrategyId { get; init; } = investmentStrategyId;
    public DateTime OccurredAtUtc { get; init; } = occurredAtUtc;
}

