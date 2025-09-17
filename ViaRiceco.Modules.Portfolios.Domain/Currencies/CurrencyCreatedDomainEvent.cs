using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.Currencies;

public sealed class CurrencyCreatedDomainEvent(string currencyId, DateTime createdAtUtc) : DomainEvent
{
    public string CurrencyId { get; init; } = currencyId;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;
}
