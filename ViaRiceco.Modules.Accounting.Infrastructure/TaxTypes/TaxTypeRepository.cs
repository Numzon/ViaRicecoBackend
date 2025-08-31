using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;
using ViaRiceco.Modules.Accounting.Infrastructure.Database;
using System.Linq.Dynamic.Core;

namespace ViaRiceco.Modules.Accounting.Infrastructure.TaxTypes;

internal sealed class TaxTypeRepository(AccountingDbContext context) : ITaxTypeRepository
{
    public Task<TaxType?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return context.TaxTypes.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TaxType>> GetPageAsync(string? search, string orderBy, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        string? cleanedSearch = search?.Trim().ToLowerInvariant();
        
        return await context.TaxTypes
            .Where(h => cleanedSearch == null || EF.Functions.ILike(h.Name, $"%{cleanedSearch}%"))
            .OrderBy(orderBy)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return context.TaxTypes.AnyAsync(t => t.Name == name, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string name, string excludeId, CancellationToken cancellationToken = default)
    {
        return context.TaxTypes.AnyAsync(t => t.Name == name && t.Id != excludeId, cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return context.TaxTypes.CountAsync(cancellationToken);
    }

    public void Insert(TaxType taxType)
    {
        context.TaxTypes.Add(taxType);
    }

    public void Delete(TaxType taxType)
    {
        context.TaxTypes.Remove(taxType);
    }
}
