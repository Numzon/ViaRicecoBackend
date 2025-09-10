using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Portfolios.Domain.Investments;
using ViaRiceco.Modules.Portfolios.Infrastructure.Database;
using System.Linq.Dynamic.Core;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.Investments;

internal sealed class InvestmentRepository(PortfoliosDbContext context) : IInvestmentRepository
{
    public Task<Investment?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return context.Investments
            .Include(i => i.PurchaseRecords)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Investment>> GetByStrategyAsync(string strategyId, CancellationToken cancellationToken = default)
    {
        return await context.Investments
            .Include(i => i.PurchaseRecords)
            .Where(i => i.InvestmentStrategyId == strategyId)
            .OrderBy(i => i.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Investment>> GetPageAsync(string? search, string orderBy, int page, int pageSize,
        string? strategyId, CancellationToken cancellationToken = default)
    {
        IQueryable<Investment> query = context.Investments
            .Include(i => i.PurchaseRecords)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i => i.Name.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(strategyId))
        {
            query = query.Where(i => i.InvestmentStrategyId == strategyId);
        }

        return await query
            .OrderBy(orderBy)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByNameAndStrategyAsync(string name, string strategyId, CancellationToken cancellationToken = default)
    {
        return context.Investments.AnyAsync(i => i.Name == name && i.InvestmentStrategyId == strategyId, cancellationToken);
    }

    public Task<bool> ExistsByNameAndStrategyAsync(string name, string strategyId, string excludeId, CancellationToken cancellationToken = default)
    {
        return context.Investments.AnyAsync(i => i.Name == name && i.InvestmentStrategyId == strategyId && i.Id != excludeId, cancellationToken);
    }

    public Task<int> CountAsync(string? search, string? strategyId, CancellationToken cancellationToken = default)
    {
        IQueryable<Investment> query = context.Investments.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i => i.Name.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(strategyId))
        {
            query = query.Where(i => i.InvestmentStrategyId == strategyId);
        }

        return query.CountAsync(cancellationToken);
    }

    public void Insert(Investment investment)
    {
        context.Investments.Add(investment);
    }

    public void Delete(Investment investment)
    {
        context.Investments.Remove(investment);
    }
}
