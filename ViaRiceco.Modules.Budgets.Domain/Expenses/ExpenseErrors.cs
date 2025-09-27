using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.Expenses;

public static class ExpenseErrors
{
    public static Error NotFound(string expenseId) => Error.NotFound(
        "Expense.NotFound", 
        $"Expense with Id '{expenseId}' was not found.");
    
    public static Error InvalidSortParameter(string? sort) => Error.Validation(
        "Expense.InvalidSortParameter", 
        $"Invalid sort parameter: '{sort}'");

    public static Error DuplicateNameInExpenseType(string name, string expenseTypeId) => Error.Conflict(
        "Expense.DuplicateNameInExpenseType", 
        $"Expense with name '{name}' already exists in expense type '{expenseTypeId}'.");

    public static Error ExpenseTypeNotFound(string expenseTypeId) => Error.Validation(
        "Expense.ExpenseTypeNotFound", 
        $"Expense type with ID '{expenseTypeId}' was not found.");
}

