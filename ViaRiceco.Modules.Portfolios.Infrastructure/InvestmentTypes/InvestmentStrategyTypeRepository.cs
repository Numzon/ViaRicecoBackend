using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentTypes;
using ViaRiceco.Modules.Portfolios.Infrastructure.Database;
using System.Linq.Dynamic.Core;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.InvestmentTypes;

internal sealed class InvestmentStrategyTypeRepository(PortfoliosDbContext context) : IInvestmentStrategyTypeRepository
{
    public Task<InvestmentStrategyType?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return context.InvestmentStrategyTypes.FirstOrDefaultAsync(ist => ist.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<InvestmentStrategyType>> GetPageAsync(string? search, string orderBy, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await context.InvestmentStrategyTypes
            .Where(ist => string.IsNullOrWhiteSpace(search) || ist.Name.Contains(search))
            .OrderBy(orderBy)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return context.InvestmentStrategyTypes.AnyAsync(ist => ist.Name == name, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string name, string excludeId, CancellationToken cancellationToken = default)
    {
        return context.InvestmentStrategyTypes.AnyAsync(ist => ist.Name == name && ist.Id != excludeId, cancellationToken);
    }

    public Task<int> CountAsync(string? search, CancellationToken cancellationToken = default)
    {
        return context.InvestmentStrategyTypes
            .Where(ist => string.IsNullOrWhiteSpace(search) || ist.Name.Contains(search))
            .CountAsync(cancellationToken);
    }

    public void Insert(InvestmentStrategyType investmentStrategyType)
    {
        context.InvestmentStrategyTypes.Add(investmentStrategyType);
    }

    public void Delete(InvestmentStrategyType investmentStrategyType)
    {
        context.InvestmentStrategyTypes.Remove(investmentStrategyType);
    }
}
