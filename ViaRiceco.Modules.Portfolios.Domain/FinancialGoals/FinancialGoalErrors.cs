using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.FinancialGoals;

public static class FinancialGoalErrors
{
    public static Error NotFound(string goalId) => Error.NotFound(
        "FinancialGoal.NotFound", 
        $"Financial goal with Id '{goalId}' was not found.");
    
    public static Error ParentNotFound(string parentId) => Error.NotFound(
        "FinancialGoal.ParentNotFound", 
        $"Parent financial goal with Id '{parentId}' was not found.");
    
    public static Error CircularReference(string goalId, string parentId) => Error.Validation(
        "FinancialGoal.CircularReference", 
        $"Cannot set parent '{parentId}' for goal '{goalId}' as it would create a circular reference.");
    
    public static Error CannotDeleteGoalWithChildren(string goalId) => Error.Conflict(
        "FinancialGoal.CannotDeleteGoalWithChildren", 
        $"Cannot delete financial goal '{goalId}' because it has child goals. Please delete or reassign child goals first.");
}
