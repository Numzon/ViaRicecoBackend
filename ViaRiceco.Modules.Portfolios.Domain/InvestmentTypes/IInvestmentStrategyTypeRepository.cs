namespace ViaRiceco.Modules.Portfolios.Domain.InvestmentTypes;

public interface IInvestmentStrategyTypeRepository
{
    Task<InvestmentStrategyType?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InvestmentStrategyType>> GetPageAsync(string? search, string orderBy, int page, int pageSize,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, string excludeId, CancellationToken cancellationToken = default);
    Task<int> CountAsync(string? search, CancellationToken cancellationToken = default);
    void Insert(InvestmentStrategyType investmentStrategyType);
    void Delete(InvestmentStrategyType investmentStrategyType);
}
