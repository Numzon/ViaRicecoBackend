using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.Incomes;

public sealed class IncomeUpdatedDomainEvent(string incomeId, decimal value, DateTime updatedAtUtc) : DomainEvent
{
    public string IncomeId { get; init; } = incomeId;
    public decimal Value { get; init; } = value;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;   
}
