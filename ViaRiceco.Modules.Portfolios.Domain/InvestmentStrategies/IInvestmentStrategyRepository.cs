namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;

public interface IInvestmentStrategyRepository
{
    Task<InvestmentStrategy?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InvestmentStrategy>> GetByFinancialGoalAsync(string financialGoalId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InvestmentStrategy>> GetByStrategyTypeAsync(string strategyTypeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InvestmentStrategy>> GetPageAsync(string? search, string orderBy, int page, int pageSize,
        string? financialGoalId, string? strategyTypeId, CancellationToken cancellationToken = default);
    Task<int> CountAsync(string? search, string? financialGoalId, string? strategyTypeId, CancellationToken cancellationToken = default);
    void Insert(InvestmentStrategy investmentStrategy);
    void Delete(InvestmentStrategy investmentStrategy);
}
