namespace ViaRiceco.Modules.Portfolios.Domain.FinancialGoals;

public static class FinancialGoalSpecification
{
    /// <summary>
    /// Determines whether the financial goal name is valid (not null or empty)
    /// </summary>
    public static bool IsValidName(string? name)
    {
        return !string.IsNullOrWhiteSpace(name);
    }

    /// <summary>
    /// Determines whether setting the specified parent ID would create a circular reference
    /// </summary>
    public static bool WouldCreateCircularReference(string goalId, string? parentId)
    {
        return parentId == goalId;
    }

    /// <summary>
    /// Determines whether the financial goal is a root goal (has no parent)
    /// </summary>
    public static bool IsRootGoal(FinancialGoal financialGoal)
    {
        return financialGoal.ParentId is null;
    }

    /// <summary>
    /// Determines whether the financial goal has a specific parent
    /// </summary>
    public static bool HasParent(FinancialGoal financialGoal, string parentId)
    {
        return financialGoal.ParentId == parentId;
    }

    /// <summary>
    /// Determines whether all financial goal creation parameters are valid
    /// </summary>
    public static bool AreCreateParametersValid(string? name)
    {
        return IsValidName(name);
    }

    /// <summary>
    /// Determines whether all financial goal update parameters are valid
    /// </summary>
    public static bool AreUpdateParametersValid(string goalId, string? name, string? parentId)
    {
        return IsValidName(name) && !WouldCreateCircularReference(goalId, parentId);
    }
}
