namespace ViaRiceco.Modules.Portfolios.Domain.InvestedCashHistories;

public interface IInvestedCashHistoryRepository
{
    Task<InvestedCashRecord?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task<InvestedCashRecord?> GetByInvestmentStrategyAndMonthlyBudgetExpenseAsync(string investmentStrategyId, string monthlyBudgetExpenseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InvestedCashRecord>> GetByInvestmentStrategyAsync(string investmentStrategyId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalInvestedCashByInvestmentStrategyAsync(string investmentStrategyId, CancellationToken cancellationToken = default);
    void Insert(InvestedCashRecord investedCashRecord);
    void Delete(InvestedCashRecord investedCashRecord);
}

