using ViaRiceco.Common.Application.EventBus;

namespace ViaRiceco.Modules.Portfolios.IntegrationEvents.InvestmentStrategies;

public sealed class InvestmentStrategyCreatedIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    string investmentStrategyId, 
    string investmentStrategyTypeId,
    IReadOnlyCollection<FinancialGoalIntegrationModel> financialGoals)
    : IntegrationEvent(id, occurredOnUtc)
{
    public string InvestmentStrategyId { get; init; } = investmentStrategyId;
    public string InvestmentStrategyTypeId { get; init; } = investmentStrategyTypeId;
    public IReadOnlyCollection<FinancialGoalIntegrationModel> FinancialGoals { get; init; } = financialGoals;
}
