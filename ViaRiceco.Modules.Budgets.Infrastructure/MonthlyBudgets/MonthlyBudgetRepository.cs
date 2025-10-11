using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;
using ViaRiceco.Modules.Budgets.Infrastructure.Database;

namespace ViaRiceco.Modules.Budgets.Infrastructure.MonthlyBudgets;

internal sealed class MonthlyBudgetRepository(BudgetsDbContext context) : IMonthlyBudgetRepository
{
    public async Task<MonthlyBudget?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return await context.MonthlyBudgets
            .FirstOrDefaultAsync(mb => mb.Id == id, cancellationToken);
    }

    public async Task<MonthlyBudget?> GetBySettlementPeriodIdAsync(string settlementPeriodId, CancellationToken cancellationToken = default)
    {
        return await context.MonthlyBudgets
            .FirstOrDefaultAsync(mb => mb.SettlementPeriodId == settlementPeriodId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<MonthlyBudget>> GetPageAsync(string? search, string orderBy, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        string? cleanedSearch = search?.Trim().ToLowerInvariant();

        return await context.MonthlyBudgets
            .Where(mb => cleanedSearch == null || 
                        EF.Functions.ILike(mb.SettlementPeriodId, $"%{cleanedSearch}%") ||
                        EF.Functions.ILike(EF.Property<string>(mb, nameof(MonthlyBudget.Month)), $"%{cleanedSearch}%") ||
                        EF.Functions.ILike(EF.Property<string>(mb, nameof(MonthlyBudget.Year)), $"%{cleanedSearch}%"))
            .OrderBy(orderBy)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<MonthlyBudget>> GetByMonthAndYearAsync(int month, int year, CancellationToken cancellationToken = default)
    {
        return await context.MonthlyBudgets
            .Where(mb => mb.Month == month && mb.Year == year)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsForSettlementPeriodAsync(string settlementPeriodId, CancellationToken cancellationToken = default)
    {
        return await context.MonthlyBudgets
            .AnyAsync(mb => mb.SettlementPeriodId == settlementPeriodId, cancellationToken);
    }

    public async Task<int> CountAsync(string? search, CancellationToken cancellationToken = default)
    {
        string? cleanedSearch = search?.Trim().ToLowerInvariant();

        return await context.MonthlyBudgets
            .Where(mb => cleanedSearch == null || 
                        EF.Functions.ILike(mb.SettlementPeriodId, $"%{cleanedSearch}%") ||
                        EF.Functions.ILike(EF.Property<string>(mb, nameof(MonthlyBudget.Month)), $"%{cleanedSearch}%") ||
                        EF.Functions.ILike(EF.Property<string>(mb, nameof(MonthlyBudget.Year)), $"%{cleanedSearch}%"))
            .CountAsync(cancellationToken);
    }

    public void Insert(MonthlyBudget monthlyBudget)
    {
        context.MonthlyBudgets.Add(monthlyBudget);
    }

    public void Delete(MonthlyBudget monthlyBudget)
    {
        context.MonthlyBudgets.Remove(monthlyBudget);
    }
}
