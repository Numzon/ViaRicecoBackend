using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.MonthlyBudgetExpenses;

public sealed class MonthlyBudgetExpenseCreatedDomainEvent(
    string monthlyBudgetExpenseId,
    string monthlyBudgetId,
    string expenseId,
    string expenseName,
    string expenseTypeId,
    string expenseTypeName,
    DateTime createdAtUtc) : DomainEvent(Guid.NewGuid(), createdAtUtc)
{
    public string MonthlyBudgetExpenseId { get; init; } = monthlyBudgetExpenseId;
    public string MonthlyBudgetId { get; init; } = monthlyBudgetId;
    public string ExpenseId { get; init; } = expenseId;
    public string ExpenseName { get; init; } = expenseName;
    public string ExpenseTypeId { get; init; } = expenseTypeId;
    public string ExpenseTypeName { get; init; } = expenseTypeName;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;
}
