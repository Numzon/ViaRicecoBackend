using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;

public sealed class ExpenseType : Entity
{
    private ExpenseType()
    {
    }

    public string Name { get; private set; } = string.Empty;

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
            CreatedAtUtc = createdAtUtc
        };

        expenseType.Raise(new ExpenseTypeCreatedDomainEvent(expenseType.Id, createdAtUtc));

        return Result.Success(expenseType);
    }

    public Result Update(string name, DateTime updatedAtUtc)
    {
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
