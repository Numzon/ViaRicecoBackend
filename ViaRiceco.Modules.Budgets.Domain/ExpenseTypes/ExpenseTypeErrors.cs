using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;

public static class ExpenseTypeErrors
{
    public static Error NotFound(string expenseTypeId) => Error.NotFound(
        "ExpenseType.NotFound", 
        $"Expense type with Id '{expenseTypeId}' was not found.");
    
    public static Error CannotUpdateSystemDefined() => Error.Validation(
        "ExpenseType.CannotUpdateSystemDefined",
        "System-defined expense types cannot be modified.");

    public static Error InvalidSortParameter(string? sort) => Error.Validation(
        "ExpenseType.InvalidSortParameter", 
        $"Invalid sort parameter: '{sort}'");

    public static Error DuplicateName(string name) => Error.Conflict(
        "ExpenseType.DuplicateName", 
        $"Expense type with name '{name}' already exists.");

    public static Error InUse(string expenseTypeId) => Error.Conflict(
        "ExpenseType.InUse",
        $"Expense type with ID '{expenseTypeId}' cannot be deleted because it is in use by one or more expenses.");

    public static Error CannotArchiveSystemDefined() => Error.Validation(
        "ExpenseType.CannotArchiveSystemDefined",
        "System-defined expense types cannot be archived.");
}

