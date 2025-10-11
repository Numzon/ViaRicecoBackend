using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

public sealed class MonthlyBudgetNetValueUpdatedDomainEvent(
    string monthlyBudgetId, 
    decimal netValue, 
    DateTime updatedAtUtc) : DomainEvent
{
    public string MonthlyBudgetId { get; init; } = monthlyBudgetId;
    public decimal NetValue { get; init; } = netValue;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;
}
