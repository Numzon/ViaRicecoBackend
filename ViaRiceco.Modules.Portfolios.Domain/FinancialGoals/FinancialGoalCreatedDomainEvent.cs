using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.FinancialGoals;

public sealed class FinancialGoalCreatedDomainEvent(string goalId, DateTime createdAtUtc) : DomainEvent
{
    public string GoalId { get; init; } = goalId;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;
}
