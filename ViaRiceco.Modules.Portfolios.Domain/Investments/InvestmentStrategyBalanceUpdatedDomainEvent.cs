using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.Investments;

public sealed class InvestmentStrategyBalanceUpdatedDomainEvent(
    string investmentStrategyId,
    DateTime createdAtUtc) : DomainEvent
{
    public string InvestmentStrategyId { get; init; } = investmentStrategyId;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;
}
