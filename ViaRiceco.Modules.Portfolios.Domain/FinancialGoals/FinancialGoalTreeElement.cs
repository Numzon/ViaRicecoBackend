namespace ViaRiceco.Modules.Portfolios.Domain.FinancialGoals;

public class FinancialGoalTreeElement
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string? ParentId { get; init; }
    public int TreeDepth { get; init; }
}
