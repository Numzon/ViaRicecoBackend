using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.Expenses;

public sealed class Expense : Entity
{
    private Expense()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string ExpenseTypeId { get; private set; } = string.Empty;

    public string? InvestmentStrategyId { get; set; }

    /// <summary>
    /// Creates an expense with auto-generated ID for manual creation
    /// </summary>
    public static Expense Create(string name, string expenseTypeId, DateTime createdAtUtc)
    {
        var expense = new Expense
        {
            Id = $"e_{Guid.NewGuid()}",
            Name = name,
            ExpenseTypeId = expenseTypeId,
            InvestmentStrategyId = null,
            CreatedAtUtc = createdAtUtc
        };

        expense.Raise(new ExpenseCreatedDomainEvent(expense.Id, expense.Name, expense.ExpenseTypeId, createdAtUtc));

        return expense;
    }

    /// <summary>
    /// Creates an expense with provided ID for integration events
    /// </summary>
    public static Expense CreateFromIntegrationEvent(
        string name, 
        string expenseTypeId, 
        string investmentStrategyId,
        DateTime createdAtUtc)
    {
        var expense = new Expense
        {
            Id = $"e_{Guid.NewGuid()}",
            Name = name,
            ExpenseTypeId = expenseTypeId,
            InvestmentStrategyId = investmentStrategyId,
            CreatedAtUtc = createdAtUtc
        };

        expense.Raise(new ExpenseCreatedFromIntegrationEventDomainEvent(expense.Id, expense.Name, expense.ExpenseTypeId, createdAtUtc));

        return expense;
    }

    public void Update(string name, string expenseTypeId, DateTime updatedAtUtc)
    {
        if (Name == name && ExpenseTypeId == expenseTypeId)
        {
            return; 
        }

        Name = name;
        ExpenseTypeId = expenseTypeId;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new ExpenseUpdatedDomainEvent(Id, Name, ExpenseTypeId, updatedAtUtc));
    }
}
