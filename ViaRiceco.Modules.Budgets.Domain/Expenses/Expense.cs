using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.Expenses;

public sealed class Expense : Entity
{
    private Expense()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string ExpenseTypeId { get; private set; } = string.Empty;
    public bool IsArchived { get; private set; }
    public string? BankId { get; private set; } 

    public string? InvestmentStrategyId { get; set; }
    
    public static Expense Create(string name, string expenseTypeId, string? bankId, DateTime createdAtUtc)
    {
        var expense = new Expense
        {
            Id = $"e_{Guid.NewGuid()}",
            Name = name,
            ExpenseTypeId = expenseTypeId,
            IsArchived = false,
            BankId = bankId,
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
            IsArchived = false,
            BankId = null,
            InvestmentStrategyId = investmentStrategyId,
            CreatedAtUtc = createdAtUtc
        };

        expense.Raise(new ExpenseCreatedFromIntegrationEventDomainEvent(expense.Id, expense.Name, expense.ExpenseTypeId, createdAtUtc));

        return expense;
    }

    public void Update(string name, string expenseTypeId, string? bankId, DateTime updatedAtUtc)
    {
        if (Name == name && ExpenseTypeId == expenseTypeId && BankId == bankId)
        {
            return; 
        }

        Name = name;
        ExpenseTypeId = expenseTypeId;
        BankId = bankId;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new ExpenseUpdatedDomainEvent(Id, Name, ExpenseTypeId, updatedAtUtc));
    }

    public void Archive(DateTime archivedAtUtc)
    {
        if (IsArchived)
        {
            return;
        }

        IsArchived = true;
        UpdatedAtUtc = archivedAtUtc;

        Raise(new ExpenseArchivedDomainEvent(Id, Name, ExpenseTypeId, archivedAtUtc));
    }

    public void Unarchive(DateTime unarchivedAtUtc)
    {
        if (!IsArchived)
        {
            return;
        }

        IsArchived = false;
        UpdatedAtUtc = unarchivedAtUtc;

        Raise(new ExpenseUnarchivedDomainEvent(Id, Name, ExpenseTypeId, unarchivedAtUtc));
    }
}
