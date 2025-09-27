namespace ViaRiceco.Modules.Portfolios.IntegrationEvents.InvestmentStrategies;

public sealed class FinancialGoalIntegrationModel
{
    public string Id { get; init; }
    public string? ParentId { get; init; }
    public string Name { get; init; }
    public int TreeDepth { get; init; }
}
