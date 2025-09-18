using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;

public sealed class ExpenseType : Entity
{
    private ExpenseType()
    {
    }

    public string Name { get; private set; } = string.Empty;
    
    /// <summary>
    /// Indicates if this expense type is defined by the system (cannot be modified/deleted by users)
    /// </summary>
    public bool IsSystemDefined { get; private set; }

    /// <summary>
    /// Creates a user-defined expense type
    /// </summary>
    public static Result<ExpenseType> Create(string name, DateTime createdAtUtc)
    {
        if (!ExpenseTypeSpecification.IsValidName(name))
        {
            return Result.Failure<ExpenseType>(ExpenseTypeErrors.InvalidName());
        }

        var expenseType = new ExpenseType
        {
            Id = $"et_{Guid.NewGuid()}",
            Name = name,
            IsSystemDefined = false, // User-created expense types are not system-defined
            CreatedAtUtc = createdAtUtc
        };

        expenseType.Raise(new ExpenseTypeCreatedDomainEvent(expenseType.Id, createdAtUtc));

        return Result.Success(expenseType);
    }

    public Result Update(string name, DateTime updatedAtUtc)
    {
        // Prevent updates to system-defined expense types
        if (IsSystemDefined)
        {
            return Result.Failure(ExpenseTypeErrors.CannotUpdateSystemDefined());
        }

        if (!ExpenseTypeSpecification.IsValidName(name))
        {
            return Result.Failure(ExpenseTypeErrors.InvalidName());
        }

        if (Name == name)
        {
            return Result.Success();
        }

        Name = name;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new ExpenseTypeUpdatedDomainEvent(Id, Name, updatedAtUtc));
        
        return Result.Success();
    }
}
