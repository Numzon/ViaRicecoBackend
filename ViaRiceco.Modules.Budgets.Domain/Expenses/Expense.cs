using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.Expenses;

public sealed class Expense : Entity
{
    private Expense()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string ExpenseTypeId { get; private set; } = string.Empty;

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
            CreatedAtUtc = createdAtUtc
        };

        expense.Raise(new ExpenseCreatedDomainEvent(expense.Id, expense.Name, expense.ExpenseTypeId, createdAtUtc));

        return expense;
    }

    /// <summary>
    /// Creates an expense with provided ID for integration events
    /// </summary>
    public static Expense CreateFromIntegrationEvent(
        string id, 
        string name, 
        string expenseTypeId, 
        DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Expense ID cannot be empty for integration events.", nameof(id));
        }

        var expense = new Expense
        {
            Id = id,
            Name = name,
            ExpenseTypeId = expenseTypeId,
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
