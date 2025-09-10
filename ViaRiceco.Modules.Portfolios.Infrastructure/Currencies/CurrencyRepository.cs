using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Portfolios.Domain.Currencies;
using ViaRiceco.Modules.Portfolios.Infrastructure.Database;
using System.Linq.Dynamic.Core;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.Currencies;

internal sealed class CurrencyRepository(PortfoliosDbContext context) : ICurrencyRepository
{
    public Task<Currency?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return context.Currencies.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return context.Currencies.FirstOrDefaultAsync(c => c.Code == code, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Currency>> GetPageAsync(string? search, string orderBy, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await context.Currencies
            .Where(c => string.IsNullOrWhiteSpace(search) || 
                       c.Name.Contains(search) || 
                       c.Code.Contains(search))
            .OrderBy(orderBy)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return context.Currencies.AnyAsync(c => c.Code == code, cancellationToken);
    }

    public Task<bool> ExistsByCodeAsync(string code, string excludeId, CancellationToken cancellationToken = default)
    {
        return context.Currencies.AnyAsync(c => c.Code == code && c.Id != excludeId, cancellationToken);
    }

    public Task<int> CountAsync(string? search, CancellationToken cancellationToken = default)
    {
        return context.Currencies
            .Where(c => string.IsNullOrWhiteSpace(search) || 
                       c.Name.Contains(search) || 
                       c.Code.Contains(search))
            .CountAsync(cancellationToken);
    }

    public void Insert(Currency currency)
    {
        context.Currencies.Add(currency);
    }

    public void Delete(Currency currency)
    {
        context.Currencies.Remove(currency);
    }
}
