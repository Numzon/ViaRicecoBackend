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
        if (string.IsNullOrWhiteSpace(code) || code.Length != 3 || !code.All(char.IsLetter))
        {
            return Result.Failure<Currency>(CurrencyErrors.InvalidCode(code));
        }

        var currency = new Currency
        {
            Id = $"cur_{Guid.NewGuid()}",
            Name = name,
            Code = code.ToUpperInvariant(), // Currency codes are typically uppercase (USD, EUR, etc.)
            CreatedAtUtc = createdAtUtc
        };

        currency.Raise(new CurrencyCreatedDomainEvent(currency.Id, createdAtUtc));

        return Result.Success(currency);
    }

    public Result Update(string name, string code, DateTime updatedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 3 || !code.All(char.IsLetter))
        {
            return Result.Failure(CurrencyErrors.InvalidCode(code));
        }

        string normalizedCode = code.ToUpperInvariant();
        
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
