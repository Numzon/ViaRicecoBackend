namespace ViaRiceco.Modules.Portfolios.Application.PurchaseRecords.Models;

public sealed record PurchaseRecordDto(
    string Id,
    DateTime PurchaseDate,
    decimal Amount,
    decimal PricePerUnit,
    decimal TotalPrice,
    string CurrencyId,
    string InvestmentId);
