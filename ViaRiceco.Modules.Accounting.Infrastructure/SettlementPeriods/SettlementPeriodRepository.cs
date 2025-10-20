using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Accounting.Domain.SettlementPeriods;
using ViaRiceco.Modules.Accounting.Infrastructure.Database;
using System.Linq.Dynamic.Core;

namespace ViaRiceco.Modules.Accounting.Infrastructure.SettlementPeriods;

internal sealed class SettlementPeriodRepository(AccountingDbContext context) : ISettlementPeriodRepository
{
    public Task<SettlementPeriod?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return context.SettlementPeriods
            .Include(sp => sp.Incomes)
            .Include(sp => sp.Taxes)
            .FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken);
    }

    public Task<SettlementPeriod?> GetByMonthAndYearAsync(int month, int year, CancellationToken cancellationToken = default)
    {
        return context.SettlementPeriods
            .Include(sp => sp.Incomes)
            .Include(sp => sp.Taxes)
            .FirstOrDefaultAsync(sp => sp.Month == month && sp.Year == year, cancellationToken);
    }

    public async Task<IReadOnlyCollection<SettlementPeriod>> GetPageAsync(string? search, string orderBy, int page, int pageSize,
        int? month, int? year, CancellationToken cancellationToken = default)
    {
        int? searchYear = null;
        int? searchMonth = null;
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            string cleanedSearch = search.Trim();
            if (int.TryParse(cleanedSearch, out int searchNumber))
            {
                searchYear = searchNumber;
                if (searchNumber >= 1 && searchNumber <= 12)
                {
                    searchMonth = searchNumber;
                }
            }
        }
        
        return await context.SettlementPeriods
            .Include(sp => sp.Incomes)
            .Include(sp => sp.Taxes)
            .Where(sp => (searchYear == null || sp.Year == searchYear || sp.Month == searchMonth) &&
                        (month == null || sp.Month == month) &&
                        (year == null || sp.Year == year))
            .OrderBy(orderBy)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByMonthAndYearAsync(int month, int year, CancellationToken cancellationToken = default)
    {
        return context.SettlementPeriods.AnyAsync(sp => sp.Month == month && sp.Year == year, cancellationToken);
    }

    public Task<bool> ExistsByMonthAndYearAsync(int month, int year, string excludeId, CancellationToken cancellationToken = default)
    {
        return context.SettlementPeriods.AnyAsync(sp => sp.Month == month && sp.Year == year && sp.Id != excludeId, cancellationToken);
    }

    public Task<SettlementPeriod?> GetMostRecentAsync(CancellationToken cancellationToken = default)
    {
        return context.SettlementPeriods
            .Include(sp => sp.Incomes)
            .Include(sp => sp.Taxes)
            .OrderByDescending(sp => sp.Year)
            .ThenByDescending(sp => sp.Month)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> IsNewerPeriodExistsAsync(int month, int year, CancellationToken cancellationToken = default)
    {
        return context.SettlementPeriods
            .AnyAsync(sp => sp.Year > year || sp.Year == year && sp.Month > month, cancellationToken);
    }

    public Task<bool> HasDraftPeriodAsync(CancellationToken cancellationToken = default)
    {
        return context.SettlementPeriods
            .AnyAsync(sp => sp.IsDraft, cancellationToken);
    }

    public Task<int> CountAsync(string? search, int? month, int? year, CancellationToken cancellationToken = default)
    {
        int? searchYear = null;
        int? searchMonth = null;
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            string cleanedSearch = search.Trim();
            if (int.TryParse(cleanedSearch, out int searchNumber))
            {
                searchYear = searchNumber;
                if (searchNumber >= 1 && searchNumber <= 12)
                {
                    searchMonth = searchNumber;
                }
            }
        }
        
        return context.SettlementPeriods
            .Where(sp => (searchYear == null || sp.Year == searchYear || sp.Month == searchMonth) &&
                        (month == null || sp.Month == month) &&
                        (year == null || sp.Year == year))
            .CountAsync(cancellationToken);
    }

    public void Insert(SettlementPeriod settlementPeriod)
    {
        context.SettlementPeriods.Add(settlementPeriod);
    }

    public void Delete(SettlementPeriod settlementPeriod)
    {
        context.SettlementPeriods.Remove(settlementPeriod);
    }
}
