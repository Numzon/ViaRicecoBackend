using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

public static class MonthlyBudgetErrors
{
    public static Error NotFound(string monthlyBudgetId) => Error.NotFound(
        "MonthlyBudget.NotFound", 
        $"Monthly budget with Id '{monthlyBudgetId}' was not found.");

    public static Error AlreadyFinalized() => Error.Conflict(
        "MonthlyBudget.AlreadyFinalized",
        "Monthly budget is already finalized and cannot be modified.");

    public static Error CannotFinalizeWithNullValues(int expensesWithoutValues) => Error.Validation(
        "MonthlyBudget.CannotFinalizeWithNullValues", 
        $"Cannot finalize monthly budget. {expensesWithoutValues} expense(s) do not have values set. All expenses must have values (including 0) before finalizing.");

    public static Error CannotModifyFinalizedBudget() => Error.Conflict(
        "MonthlyBudget.CannotModifyFinalizedBudget",
        "Cannot modify a finalized monthly budget. Set it as draft first if changes are needed.");

    public static Error ExpenseAlreadyExists(string expenseId) => Error.Conflict(
        "MonthlyBudget.ExpenseAlreadyExists", 
        $"Expense with ID '{expenseId}' already exists in this monthly budget.");

    public static Error ExpenseNotFound(string expenseId) => Error.NotFound(
        "MonthlyBudget.ExpenseNotFound", 
        $"Expense with ID '{expenseId}' was not found in this monthly budget.");

    public static Error InvalidMonth(int month) => Error.Validation(
        "MonthlyBudget.InvalidMonth", 
        $"Month must be between 1 and 12, but was {month}");

    public static Error InvalidYear(int year) => Error.Validation(
        "MonthlyBudget.InvalidYear", 
        $"Year must be between 1900 and 2100, but was {year}");

    public static Error AlreadyExistsForPeriod(int month, int year) => Error.Conflict(
        "MonthlyBudget.AlreadyExistsForPeriod", 
        $"A monthly budget for {month:D2}/{year} already exists");

    public static Error CannotSetAsNonRecentDraft(int month, int year) => Error.Conflict(
        "MonthlyBudget.CannotSetAsNonRecentDraft", 
        $"Cannot set monthly budget {month:D2}/{year} as draft. Only the most recent monthly budget can be set as draft");
}
