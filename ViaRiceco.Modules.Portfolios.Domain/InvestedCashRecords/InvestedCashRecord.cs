using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Portfolios.Domain.InvestedCashRecords;

public sealed class InvestedCashRecord : Entity
{
    private InvestedCashRecord()
    {
    }

    public string InvestmentStrategyId { get; private set; } = string.Empty;
    public string MonthlyBudgetExpenseId { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }

    public static InvestedCashRecord Create(
        string investmentStrategyId,
        string monthlyBudgetExpenseId,
        decimal amount,
        DateTime createdAtUtc)
    {
        var investedCashRecord = new InvestedCashRecord
        {
            Id = $"icr_{Guid.NewGuid()}",
            InvestmentStrategyId = investmentStrategyId,
            MonthlyBudgetExpenseId = monthlyBudgetExpenseId,
            Amount = amount,
            CreatedAtUtc = createdAtUtc
        };

        return investedCashRecord;
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

