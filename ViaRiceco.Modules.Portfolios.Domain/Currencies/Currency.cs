using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.Currencies;

public sealed class Currency : Entity
{
    private Currency()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;

    public static Result<Currency> Create(string name, string code, DateTime createdAtUtc)
    {
        if (!CurrencySpecification.AreCreateParametersValid(name, code))
        {
            if (!CurrencySpecification.IsValidName(name))
            {
                return Result.Failure<Currency>(CurrencyErrors.InvalidName());
            }
            
            return Result.Failure<Currency>(CurrencyErrors.InvalidCode());
        }

        var currency = new Currency
        {
            Id = $"c_{Guid.NewGuid()}",
            Name = name,
            Code = CurrencySpecification.NormalizeCode(code),
            CreatedAtUtc = createdAtUtc
        };

        currency.Raise(new CurrencyCreatedDomainEvent(currency.Id, createdAtUtc));

        return Result.Success(currency);
    }

    public Result Update(string name, string code, DateTime updatedAtUtc)
    {
        if (!CurrencySpecification.AreUpdateParametersValid(name, code))
        {
            if (!CurrencySpecification.IsValidName(name))
            {
                return Result.Failure(CurrencyErrors.InvalidName());
            }
            
            return Result.Failure(CurrencyErrors.InvalidCode());
        }

        string normalizedCode = CurrencySpecification.NormalizeCode(code);
        
        if (Name == name && Code == normalizedCode)
        {
            return Result.Success();
        }

        Name = name;
        Code = normalizedCode;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new CurrencyUpdatedDomainEvent(Id, Name, Code, updatedAtUtc));
        
        return Result.Success();
    }
}
