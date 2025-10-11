using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;
using ViaRiceco.Modules.Budgets.Infrastructure.Database;
using System.Linq.Dynamic.Core;

namespace ViaRiceco.Modules.Budgets.Infrastructure.ExpenseTypes;

internal sealed class ExpenseTypeRepository(BudgetsDbContext context) : IExpenseTypeRepository
{
    public Task<ExpenseType?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return context.ExpenseTypes.FirstOrDefaultAsync(et => et.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ExpenseType>> GetByIdsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        return await context.ExpenseTypes
            .Where(et => ids.Contains(et.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ExpenseType>> GetPageAsync(string? search, string orderBy, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        string? cleanedSearch = search?.Trim().ToLowerInvariant();
        
        return await context.ExpenseTypes
            .Where(et => cleanedSearch == null || EF.Functions.ILike(et.Name, $"%{cleanedSearch}%"))
            .OrderBy(orderBy)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return context.ExpenseTypes.AnyAsync(et => et.Name == name, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string name, string excludeId, CancellationToken cancellationToken = default)
    {
        return context.ExpenseTypes.AnyAsync(et => et.Name == name && et.Id != excludeId, cancellationToken);
    }

    public Task<int> CountAsync(string? search, CancellationToken cancellationToken = default)
    {
        string? cleanedSearch = search?.Trim().ToLowerInvariant();
        
        return context.ExpenseTypes
            .Where(et => cleanedSearch == null || EF.Functions.ILike(et.Name, $"%{cleanedSearch}%"))
            .CountAsync(cancellationToken);
    }

    public void Insert(ExpenseType expenseType)
    {
        context.ExpenseTypes.Add(expenseType);
    }

    public void Delete(ExpenseType expenseType)
    {
        context.ExpenseTypes.Remove(expenseType);
    }
}
