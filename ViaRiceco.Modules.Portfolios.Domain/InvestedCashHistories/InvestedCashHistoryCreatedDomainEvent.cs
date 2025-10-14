using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.InvestedCashHistories;

public sealed class InvestedCashHistoryCreatedDomainEvent(
    string investedCashHistoryId,
    string investmentStrategyId,
    string monthlyBudgetExpenseId,
    decimal amount,
    DateTime createdAtUtc) : DomainEvent(Guid.NewGuid(), createdAtUtc)
{
    public string InvestedCashHistoryId { get; init; } = investedCashHistoryId;
    public string InvestmentStrategyId { get; init; } = investmentStrategyId;
    public string MonthlyBudgetExpenseId { get; init; } = monthlyBudgetExpenseId;
    public decimal Amount { get; init; } = amount;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;
}
