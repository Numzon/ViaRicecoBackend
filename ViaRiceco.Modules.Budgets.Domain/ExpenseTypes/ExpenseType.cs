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
    /// Indicates if this expense type is archived (soft deleted but preserves historical data)
    /// </summary>
    public bool IsArchived { get; private set; }

    /// <summary>
    /// Creates a user-defined expense type
    /// </summary>
    public static ExpenseType Create(string name, DateTime createdAtUtc)
    {
        var expenseType = new ExpenseType
        {
            Id = $"et_{Guid.NewGuid()}",
            Name = name,
            IsSystemDefined = false,
            IsArchived = false,
            CreatedAtUtc = createdAtUtc
        };

        expenseType.Raise(new ExpenseTypeCreatedDomainEvent(expenseType.Id, createdAtUtc));

        return expenseType;
    }

    public Result Update(string name, DateTime updatedAtUtc)
    {
        if (IsSystemDefined)
        {
            return Result.Failure(ExpenseTypeErrors.CannotUpdateSystemDefined());
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

    /// <summary>
    /// Archives the expense type and all related expenses (soft delete)
    /// </summary>
    public Result Archive(DateTime archivedAtUtc)
    {
        if (IsSystemDefined)
        {
            return Result.Failure(ExpenseTypeErrors.CannotArchiveSystemDefined());
        }

        if (IsArchived)
        {
            return Result.Success();
        }

        IsArchived = true;
        UpdatedAtUtc = archivedAtUtc;

        Raise(new ExpenseTypeArchivedDomainEvent(Id, Name, archivedAtUtc));
        
        return Result.Success();
    }

    /// <summary>
    /// Unarchives the expense type (restores from soft delete)
    /// </summary>
    public void Unarchive(DateTime unarchivedAtUtc)
    {
        if (!IsArchived)
        {
            return;
        }

        IsArchived = false;
        UpdatedAtUtc = unarchivedAtUtc;

        Raise(new ExpenseTypeUnarchivedDomainEvent(Id, Name, unarchivedAtUtc));
    }
}
