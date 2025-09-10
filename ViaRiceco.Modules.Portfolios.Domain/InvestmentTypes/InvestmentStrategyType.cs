using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentTypes;

public sealed class InvestmentStrategyType : Entity
{
    private InvestmentStrategyType()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public static Result<InvestmentStrategyType> Create(string name, DateTime createdAtUtc)
    {
        if (!InvestmentStrategyTypeSpecification.IsValidName(name))
        {
            return Result.Failure<InvestmentStrategyType>(InvestmentStrategyTypeErrors.InvalidName(name));
        }

        var strategyType = new InvestmentStrategyType
        {
            Id = $"ist_{Guid.NewGuid()}",
            Name = name,
            CreatedAtUtc = createdAtUtc
        };

        strategyType.Raise(new InvestmentStrategyTypeCreatedDomainEvent(strategyType.Id, createdAtUtc));

        return Result.Success(strategyType);
    }

    public Result Update(string name, DateTime updatedAtUtc)
    {
        if (!InvestmentStrategyTypeSpecification.IsValidName(name))
        {
            return Result.Failure(InvestmentStrategyTypeErrors.InvalidName(name));
        }

        if (Name == name)
        {
            return Result.Success();
        }

        Name = name;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new InvestmentStrategyTypeUpdatedDomainEvent(Id, Name, updatedAtUtc));
        
        return Result.Success();
    }
}
