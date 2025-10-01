using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.MonthlyBudgetExpenses;

public static class MonthlyBudgetExpenseErrors
{
    public static Error NotFound(string monthlyBudgetExpenseId) => Error.NotFound(
        "MonthlyBudgetExpense.NotFound", 
        $"Monthly budget expense with Id '{monthlyBudgetExpenseId}' was not found.");

    public static Error InvalidValue(decimal? value) => Error.Validation(
        "MonthlyBudgetExpense.InvalidValue", 
        $"Monthly budget expense value cannot be negative: {value}");
}
