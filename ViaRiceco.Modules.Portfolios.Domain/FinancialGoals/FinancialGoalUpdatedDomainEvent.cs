using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.FinancialGoals;

public sealed class FinancialGoalUpdatedDomainEvent(string goalId, string name, DateTime updatedAtUtc) : DomainEvent
{
    public string GoalId { get; init; } = goalId;
    public string Name { get; init; } = name;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;
}
