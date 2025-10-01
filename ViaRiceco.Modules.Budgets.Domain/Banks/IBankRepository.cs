namespace ViaRiceco.Modules.Budgets.Domain.Banks;

public interface IBankRepository
{
    Task<Bank?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Bank>> GetPageAsync(string? search, string orderBy, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, string excludeId, CancellationToken cancellationToken = default);
    Task<int> CountAsync(string? search, CancellationToken cancellationToken = default);
    Task<bool> IsInUseAsync(string bankId, CancellationToken cancellationToken = default);
    void Insert(Bank bank);
    void Delete(Bank bank);
}
