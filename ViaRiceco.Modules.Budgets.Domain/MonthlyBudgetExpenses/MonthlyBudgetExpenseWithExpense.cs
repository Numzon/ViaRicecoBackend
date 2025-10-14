namespace ViaRiceco.Modules.Budgets.Domain.MonthlyBudgetExpenses;

public sealed record MonthlyBudgetExpenseWithExpense(
    string MonthlyBudgetExpenseId,
    decimal? Value,
    string ExpenseId,
    string ExpenseTypeId,
    string? InvestmentStrategyId);
