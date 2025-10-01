using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

public sealed class MonthlyBudgetFinalizedDomainEvent(
    string monthlyBudgetId,
    decimal totalBudgetedAmount,
    DateTime finalizedAtUtc) : DomainEvent
{
    public string MonthlyBudgetId { get; init; } = monthlyBudgetId;
    public decimal TotalBudgetedAmount { get; init; } = totalBudgetedAmount;
    public DateTime FinalizedAtUtc { get; init; } = finalizedAtUtc;
}
