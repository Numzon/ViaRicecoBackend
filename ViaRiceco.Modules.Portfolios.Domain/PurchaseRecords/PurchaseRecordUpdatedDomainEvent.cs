using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.PurchaseRecords;

public sealed class PurchaseRecordUpdatedDomainEvent(
    string recordId, 
    DateTime purchaseDate, 
    decimal amount, 
    decimal pricePerUnit, 
    decimal totalPrice, 
    string currencyId, 
    decimal? currencyConvertValue,
    DateTime updatedAtUtc) : DomainEvent
{
    public string RecordId { get; init; } = recordId;
    public DateTime PurchaseDate { get; init; } = purchaseDate;
    public decimal Amount { get; init; } = amount;
    public decimal PricePerUnit { get; init; } = pricePerUnit;
    public decimal TotalPrice { get; init; } = totalPrice;
    public string CurrencyId { get; init; } = currencyId;
    public decimal? CurrencyConvertValue { get; init; } = currencyConvertValue;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;
}
