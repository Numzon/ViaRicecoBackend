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
    public static Result<Expense> Create(string name, string expenseTypeId, DateTime createdAtUtc)
    {
        if (!ExpenseSpecification.AreCreateParametersValid(name, expenseTypeId))
        {
            if (!ExpenseSpecification.IsValidName(name))
            {
                return Result.Failure<Expense>(ExpenseErrors.InvalidName());
            }
            
            return Result.Failure<Expense>(ExpenseErrors.InvalidExpenseTypeId());
        }

        var expense = new Expense
        {
            Id = $"e_{Guid.NewGuid()}",
            Name = name,
            ExpenseTypeId = expenseTypeId,
            CreatedAtUtc = createdAtUtc
        };

        expense.Raise(new ExpenseCreatedDomainEvent(expense.Id, expense.Name, expense.ExpenseTypeId, createdAtUtc));

        return Result.Success(expense);
    }

    /// <summary>
    /// Creates an expense with provided ID for integration events
    /// </summary>
    public static Result<Expense> CreateFromIntegrationEvent(
        string id, 
        string name, 
        string expenseTypeId, 
        DateTime createdAtUtc)
    {
        if (!ExpenseSpecification.AreCreateParametersValid(name, expenseTypeId))
        {
            if (!ExpenseSpecification.IsValidName(name))
            {
                return Result.Failure<Expense>(ExpenseErrors.InvalidName());
            }
            
            return Result.Failure<Expense>(ExpenseErrors.InvalidExpenseTypeId());
        }

        if (!ExpenseSpecification.IsValidId(id))
        {
            return Result.Failure<Expense>(ExpenseErrors.InvalidId());
        }

        var expense = new Expense
        {
            Id = id,
            Name = name,
            ExpenseTypeId = expenseTypeId,
            CreatedAtUtc = createdAtUtc
        };

        expense.Raise(new ExpenseCreatedDomainEvent(expense.Id, expense.Name, expense.ExpenseTypeId, createdAtUtc));

        return Result.Success(expense);
    }

    public Result Update(string name, string expenseTypeId, DateTime updatedAtUtc)
    {
        if (!ExpenseSpecification.AreUpdateParametersValid(name, expenseTypeId))
        {
            if (!ExpenseSpecification.IsValidName(name))
            {
                return Result.Failure(ExpenseErrors.InvalidName());
            }
            
            return Result.Failure(ExpenseErrors.InvalidExpenseTypeId());
        }

        if (Name == name && ExpenseTypeId == expenseTypeId)
        {
            return Result.Success();
        }

        Name = name;
        ExpenseTypeId = expenseTypeId;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new ExpenseUpdatedDomainEvent(Id, Name, ExpenseTypeId, updatedAtUtc));
        
        return Result.Success();
    }
}
