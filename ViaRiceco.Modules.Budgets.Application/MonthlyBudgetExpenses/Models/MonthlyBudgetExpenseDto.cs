namespace ViaRiceco.Modules.Budgets.Application.MonthlyBudgetExpenses.Models;

public sealed record MonthlyBudgetExpenseDto(
    string Id,
    string MonthlyBudgetId,
    string ExpenseId,
    string ExpenseName,
    string ExpenseTypeId,
    string ExpenseTypeName,
    decimal? Value,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
