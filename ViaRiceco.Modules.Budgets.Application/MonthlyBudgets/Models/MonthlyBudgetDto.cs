using ViaRiceco.Modules.Budgets.Application.MonthlyBudgetExpenses.Models;

namespace ViaRiceco.Modules.Budgets.Application.MonthlyBudgets.Models;

public sealed record MonthlyBudgetDto(
    string Id,
    string SettlementPeriodId,
    int Month,
    int Year,
    bool IsDraft,
    decimal NetValue,
    IReadOnlyCollection<MonthlyBudgetExpenseDto> Expenses,
    decimal TotalBudgetedAmount,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
