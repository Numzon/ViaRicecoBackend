namespace ViaRiceco.Modules.Accounting.Domain.TaxTypes;

public interface ITaxTypeRepository
{
    Task<TaxType?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TaxType>> GetPageAsync(string? search, string orderBy, int page, int pageSize,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, string excludeId, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    void Insert(TaxType taxType);
    void Delete(TaxType taxType);
}
