using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;

public static class ExpenseTypeErrors
{
    public static Error NotFound(string expenseTypeId) => Error.NotFound(
        "ExpenseType.NotFound", 
        $"Expense type with Id '{expenseTypeId}' was not found.");
    
    public static Error InvalidName() => Error.Validation(
        "ExpenseType.InvalidName",
        "Expense type name cannot be empty.");

    public static Error CannotUpdateSystemDefined() => Error.Validation(
        "ExpenseType.CannotUpdateSystemDefined",
        "System-defined expense types cannot be modified.");

    public static Error InvalidSortParameter(string? sort) => Error.Validation(
        "ExpenseType.InvalidSortParameter", 
        $"Invalid sort parameter: '{sort}'");

    public static Error DuplicateName(string name) => Error.Conflict(
        "ExpenseType.DuplicateName", 
        $"Expense type with name '{name}' already exists.");
}

