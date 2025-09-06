namespace ViaRiceco.Modules.Accounting.Application.SettlementPeriods.Models;

public sealed record SettlementPeriodDto(
    string Id, 
    int Month, 
    int Year, 
    decimal TotalIncome, 
    decimal TotalTaxes, 
    decimal NetAmount,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    IReadOnlyCollection<IncomeDto> Incomes,
    IReadOnlyCollection<TaxDto> Taxes);

public sealed record IncomeDto(string Id, decimal Value, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);

public sealed record TaxDto(string Id, decimal Value, string TaxTypeId, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);
