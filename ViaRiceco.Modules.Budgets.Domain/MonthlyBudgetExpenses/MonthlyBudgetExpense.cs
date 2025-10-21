using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.MonthlyBudgetExpenses;

public sealed class MonthlyBudgetExpense : Entity
{
    private MonthlyBudgetExpense()
    {
    }

    public string MonthlyBudgetId { get; private set; } = string.Empty;
    public string ExpenseId { get; private set; } = string.Empty;
    public string ExpenseName { get; private set; } = string.Empty;
    public string ExpenseTypeId { get; private set; } = string.Empty;
    public string ExpenseTypeName { get; private set; } = string.Empty;
    public decimal? Value { get; private set; }

    public static MonthlyBudgetExpense Create(
        string monthlyBudgetId,
        string expenseId,
        string expenseName,
        string expenseTypeId,
        string expenseTypeName,
        DateTime createdAtUtc)
    {
        var monthlyBudgetExpense = new MonthlyBudgetExpense
        {
            Id = $"mbe_{Guid.NewGuid()}",
            MonthlyBudgetId = monthlyBudgetId,
            ExpenseId = expenseId,
            ExpenseName = expenseName,
            ExpenseTypeId = expenseTypeId,
            ExpenseTypeName = expenseTypeName,
            Value = null, // Starts as null, must be set before budget can be finalized
            CreatedAtUtc = createdAtUtc
        };

        monthlyBudgetExpense.Raise(new MonthlyBudgetExpenseCreatedDomainEvent(
            monthlyBudgetExpense.Id,
            monthlyBudgetId,
            expenseId,
            expenseName,
            expenseTypeId,
            expenseTypeName,
            createdAtUtc));

        return monthlyBudgetExpense;
    }

    public void UpdateExpenseName(string expenseName, DateTime updatedAtUtc)
    {
        if (ExpenseName == expenseName)
        {
            return;
        }

        ExpenseName = expenseName;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new MonthlyBudgetExpenseUpdatedDomainEvent(Id, MonthlyBudgetId, ExpenseId, expenseName, updatedAtUtc));
    }

    public void SetValue(decimal? value, DateTime updatedAtUtc)
    {
        if (Value == value)
        {
            return;
        }

        Value = value;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new MonthlyBudgetExpenseValueSetDomainEvent(Id, MonthlyBudgetId, value, updatedAtUtc));
    }
}
