namespace ViaRiceco.Modules.Accounting.Domain.TaxTypes;

public interface ITaxTypeRepository
{
    Task<TaxType?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    void Insert(TaxType taxType);
}
