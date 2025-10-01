using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Budgets.Domain.Banks;
using ViaRiceco.Modules.Budgets.Infrastructure.Database;
using System.Linq.Dynamic.Core;

namespace ViaRiceco.Modules.Budgets.Infrastructure.Banks;

internal sealed class BankRepository(BudgetsDbContext context) : IBankRepository
{
    public async Task<Bank?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return await context.Banks.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Bank>> GetPageAsync(string? search, string orderBy, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        string? cleanedSearch = search?.Trim().ToLowerInvariant();

        return await context.Banks
            .Where(b => cleanedSearch == null || EF.Functions.ILike(b.Name, $"%{cleanedSearch}%"))
            .OrderBy(orderBy)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await context.Banks.AnyAsync(b => b.Name == name, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, string excludeId, CancellationToken cancellationToken = default)
    {
        return await context.Banks.AnyAsync(b => b.Name == name && b.Id != excludeId, cancellationToken);
    }

    public async Task<int> CountAsync(string? search, CancellationToken cancellationToken = default)
    {
        string? cleanedSearch = search?.Trim().ToLowerInvariant();

        return await context.Banks
            .Where(b => cleanedSearch == null || EF.Functions.ILike(b.Name, $"%{cleanedSearch}%"))
            .CountAsync(cancellationToken);
    }

    public async Task<bool> IsInUseAsync(string bankId, CancellationToken cancellationToken = default)
    {
        // Check if any expenses are using this bank
        return await context.Expenses.AnyAsync(e => e.BankId == bankId, cancellationToken);
    }

    public void Insert(Bank bank)
    {
        context.Banks.Add(bank);
    }

    public void Delete(Bank bank)
    {
        context.Banks.Remove(bank);
    }
}
