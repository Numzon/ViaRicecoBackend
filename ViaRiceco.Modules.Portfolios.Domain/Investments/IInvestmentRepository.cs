namespace ViaRiceco.Modules.Portfolios.Domain.Investments;

public interface IInvestmentRepository
{
    Task<Investment?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Investment>> GetByStrategyAsync(string strategyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Investment>> GetPageAsync(string? search, string orderBy, int page, int pageSize,
        string? strategyId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAndStrategyAsync(string name, string strategyId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAndStrategyAsync(string name, string strategyId, string excludeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithStrategyAsync(string investmentId, string strategyId, CancellationToken cancellationToken = default);
    Task<int> CountAsync(string? search, string? strategyId, CancellationToken cancellationToken = default);
    void Insert(Investment investment);
    void Delete(Investment investment);
}
