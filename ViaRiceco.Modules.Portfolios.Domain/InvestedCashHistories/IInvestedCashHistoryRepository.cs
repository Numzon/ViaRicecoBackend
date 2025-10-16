namespace ViaRiceco.Modules.Portfolios.Domain.InvestedCashHistories;

public interface IInvestedCashHistoryRepository
{
    Task<InvestedCashHistory?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task<InvestedCashHistory?> GetByInvestmentStrategyAndMonthlyBudgetExpenseAsync(string investmentStrategyId, string monthlyBudgetExpenseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InvestedCashHistory>> GetByInvestmentStrategyAsync(string investmentStrategyId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalInvestedCashByInvestmentStrategyAsync(string investmentStrategyId, CancellationToken cancellationToken = default);
    void Insert(InvestedCashHistory investedCashHistory);
    void Delete(InvestedCashHistory investedCashHistory);
}

