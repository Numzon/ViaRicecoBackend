namespace ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

public sealed record ExpenseValueUpdate(string MonthlyBudgetExpenseId, decimal? Value);
