using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.InvestedCashHistories;

public sealed class InvestedCashHistory : Entity
{
    private InvestedCashHistory()
    {
    }

    public string InvestmentStrategyId { get; private set; } = string.Empty;
    public string MonthlyBudgetExpenseId { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }

    public static InvestedCashHistory Create(
        string investmentStrategyId,
        string monthlyBudgetExpenseId,
        decimal amount,
        DateTime createdAtUtc)
    {
        var investedCashHistory = new InvestedCashHistory
        {
            Id = $"ich_{Guid.NewGuid()}",
            InvestmentStrategyId = investmentStrategyId,
            MonthlyBudgetExpenseId = monthlyBudgetExpenseId,
            Amount = amount,
            CreatedAtUtc = createdAtUtc
        };

        return investedCashHistory;
    }

    public void UpdateAmount(decimal amount, DateTime updatedAtUtc)
    {
        if (Amount == amount)
        {
            return;
        }

        Amount = amount;
        UpdatedAtUtc = updatedAtUtc;
    }
}