using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

public sealed class MonthlyBudgetSetAsDraftDomainEvent(
    string monthlyBudgetId,
    DateTime updatedAtUtc) : DomainEvent
{
    public string MonthlyBudgetId { get; init; } = monthlyBudgetId;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;
}
