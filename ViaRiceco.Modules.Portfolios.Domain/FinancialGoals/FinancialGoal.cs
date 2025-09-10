using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.FinancialGoals;

public sealed class FinancialGoal : Entity
{
    private FinancialGoal()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string? ParentId { get; private set; }

    public static Result<FinancialGoal> Create(string name, string? parentId, DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<FinancialGoal>(Error.Validation("FinancialGoal.InvalidName", "Financial goal name cannot be empty."));
        }

        var goal = new FinancialGoal
        {
            Id = $"fg_{Guid.NewGuid()}",
            Name = name,
            ParentId = parentId,
            CreatedAtUtc = createdAtUtc
        };

        goal.Raise(new FinancialGoalCreatedDomainEvent(goal.Id, createdAtUtc));

        return Result.Success(goal);
    }

    public Result Update(string name, DateTime updatedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(Error.Validation("FinancialGoal.InvalidName", "Financial goal name cannot be empty."));
        }

        if (Name == name)
        {
            return Result.Success();
        }

        Name = name;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new FinancialGoalUpdatedDomainEvent(Id, Name, updatedAtUtc));
        
        return Result.Success();
    }
}
