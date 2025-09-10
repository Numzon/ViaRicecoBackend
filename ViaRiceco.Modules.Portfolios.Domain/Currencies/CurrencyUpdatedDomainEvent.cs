using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.Currencies;

public sealed class CurrencyUpdatedDomainEvent(string currencyId, string name, string code, DateTime updatedAtUtc) : DomainEvent
{
    public string CurrencyId { get; init; } = currencyId;
    public string Name { get; init; } = name;
    public string Code { get; init; } = code;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;
}
