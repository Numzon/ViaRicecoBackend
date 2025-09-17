using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategies;
using ViaRiceco.Modules.Portfolios.Infrastructure.Database;
using System.Linq.Dynamic.Core;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.InvestmentStrategies;

internal sealed class InvestmentStrategyRepository(PortfoliosDbContext context) : IInvestmentStrategyRepository
{
    public Task<InvestmentStrategy?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return context.InvestmentStrategies
            .Include(strategy => strategy.Investments)
                .ThenInclude(investment => investment.PurchaseRecords)
            .FirstOrDefaultAsync(strategy => strategy.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<InvestmentStrategy>> GetByFinancialGoalAsync(string financialGoalId, CancellationToken cancellationToken = default)
    {
        return await context.InvestmentStrategies
            .Include(strategy => strategy.Investments)
                .ThenInclude(investment => investment.PurchaseRecords)
            .Where(strategy => strategy.FinancialGoalId == financialGoalId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<InvestmentStrategy>> GetByStrategyTypeAsync(string strategyTypeId, CancellationToken cancellationToken = default)
    {
        return await context.InvestmentStrategies
            .Include(strategy => strategy.Investments)
                .ThenInclude(investment => investment.PurchaseRecords)
            .Where(strategy => strategy.InvestmentStrategyTypeId == strategyTypeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<InvestmentStrategy>> GetPageAsync(string? search, string orderBy, int page, int pageSize,
        string? financialGoalId, string? strategyTypeId, CancellationToken cancellationToken = default)
    {
        IQueryable<InvestmentStrategy> query = context.InvestmentStrategies
            .Include(strategy => strategy.Investments)
                .ThenInclude(investment => investment.PurchaseRecords)
            .AsQueryable();

        // Apply filters - search is limited since there's no name on InvestmentStrategy
        if (!string.IsNullOrWhiteSpace(search))
        {
            string cleanedSearch = search.Trim().ToLowerInvariant();
            
            if (decimal.TryParse(cleanedSearch, System.Globalization.CultureInfo.InvariantCulture, out decimal searchAmount))
            {
                query = query.Where(strategy => strategy.UninvestedAmount == searchAmount);
            }
            else
            {
                // If not a decimal, search in related string IDs
                query = query.Where(strategy => 
                    EF.Functions.ILike(strategy.FinancialGoalId, $"%{cleanedSearch}%") ||
                    EF.Functions.ILike(strategy.InvestmentStrategyTypeId, $"%{cleanedSearch}%"));
            }
        }

        if (!string.IsNullOrWhiteSpace(financialGoalId))
        {
            query = query.Where(strategy => strategy.FinancialGoalId == financialGoalId);
        }

        if (!string.IsNullOrWhiteSpace(strategyTypeId))
        {
            query = query.Where(strategy => strategy.InvestmentStrategyTypeId == strategyTypeId);
        }

        return await query
            .OrderBy(orderBy)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(string? search, string? financialGoalId, string? strategyTypeId, CancellationToken cancellationToken = default)
    {
        IQueryable<InvestmentStrategy> query = context.InvestmentStrategies.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            string cleanedSearch = search.Trim().ToLowerInvariant();
            
            if (decimal.TryParse(cleanedSearch, System.Globalization.CultureInfo.InvariantCulture, out decimal searchAmount))
            {
                query = query.Where(strategy => strategy.UninvestedAmount == searchAmount);
            }
            else
            {
                // If not a decimal, search in related string IDs
                query = query.Where(strategy => 
                    EF.Functions.ILike(strategy.FinancialGoalId, $"%{cleanedSearch}%") ||
                    EF.Functions.ILike(strategy.InvestmentStrategyTypeId, $"%{cleanedSearch}%"));
            }
        }

        if (!string.IsNullOrWhiteSpace(financialGoalId))
        {
            query = query.Where(strategy => strategy.FinancialGoalId == financialGoalId);
        }

        if (!string.IsNullOrWhiteSpace(strategyTypeId))
        {
            query = query.Where(strategy => strategy.InvestmentStrategyTypeId == strategyTypeId);
        }

        return query.CountAsync(cancellationToken);
    }

    public void Insert(InvestmentStrategy investmentStrategy)
    {
        context.InvestmentStrategies.Add(investmentStrategy);
    }

    public void Delete(InvestmentStrategy investmentStrategy)
    {
        context.InvestmentStrategies.Remove(investmentStrategy);
    }
}
