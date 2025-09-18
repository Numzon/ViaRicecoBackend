using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.Expenses;

public static class ExpenseErrors
{
    public static Error NotFound(string expenseId) => Error.NotFound(
        "Expense.NotFound", 
        $"Expense with Id '{expenseId}' was not found.");
    
    public static Error InvalidName() => Error.Validation(
        "Expense.InvalidName",
        "Expense name cannot be empty.");

    public static Error InvalidExpenseTypeId() => Error.Validation(
        "Expense.InvalidExpenseTypeId", 
        "Expense type ID cannot be empty.");

    public static Error InvalidId() => Error.Validation(
        "Expense.InvalidId", 
        "Expense ID cannot be empty.");

    public static Error InvalidSortParameter(string? sort) => Error.Validation(
        "Expense.InvalidSortParameter", 
        $"Invalid sort parameter: '{sort}'");

    public static Error ExpenseTypeNotFound(string expenseTypeId) => Error.Validation(
        "Expense.ExpenseTypeNotFound", 
        $"Expense type with ID '{expenseTypeId}' was not found.");
}

