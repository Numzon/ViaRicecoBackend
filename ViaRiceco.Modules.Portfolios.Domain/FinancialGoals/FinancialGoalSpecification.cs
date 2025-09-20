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
    /// Determines whether the financial goal is a root goal (has no parent)
    /// </summary>
    public static bool IsRootGoal(FinancialGoal financialGoal)
    {
        return financialGoal.ParentId is null;
    }
    
    /// <summary>
    /// Determines whether a parent ID requires validation (not null and different from current)
    /// </summary>
    public static bool RequiresParentValidation(FinancialGoal currentGoal, string? proposedParentId)
    {
        return proposedParentId is not null && proposedParentId != currentGoal.ParentId;
    }

    /// <summary>
    /// Determines whether the basic update parameters are valid (name validation only)
    /// Note: Circular reference validation requires async repository access
    /// </summary>
    public static bool AreBasicUpdateParametersValid(string? name)
    {
        return IsValidName(name);
    }
}
