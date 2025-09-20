using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Budgets.Domain.Expenses;
using ViaRiceco.Modules.Budgets.Infrastructure.Database;
using System.Linq.Dynamic.Core;

namespace ViaRiceco.Modules.Budgets.Infrastructure.Expenses;

internal sealed class ExpenseRepository(BudgetsDbContext context) : IExpenseRepository
{
    public Task<Expense?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return context.Expenses.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Expense>> GetByExpenseTypeAsync(string expenseTypeId, CancellationToken cancellationToken = default)
    {
        return await context.Expenses
            .Where(e => e.ExpenseTypeId == expenseTypeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Expense>> GetPageAsync(string? search, string orderBy, int page, int pageSize,
        string? expenseTypeId, CancellationToken cancellationToken = default)
    {
        string? cleanedSearch = search?.Trim().ToLowerInvariant();
        
        return await context.Expenses
            .Where(e => (cleanedSearch == null || EF.Functions.ILike(e.Name, $"%{cleanedSearch}%")) &&
                       (expenseTypeId == null || e.ExpenseTypeId == expenseTypeId))
            .OrderBy(orderBy)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByNameAndExpenseTypeAsync(string name, string expenseTypeId, CancellationToken cancellationToken = default)
    {
        return context.Expenses.AnyAsync(e => e.Name == name && e.ExpenseTypeId == expenseTypeId, cancellationToken);
    }

    public Task<bool> ExistsByNameAndExpenseTypeAsync(string name, string expenseTypeId, string excludeId, CancellationToken cancellationToken = default)
    {
        return context.Expenses.AnyAsync(e => e.Name == name && e.ExpenseTypeId == expenseTypeId && e.Id != excludeId, cancellationToken);
    }

    public Task<bool> ExistsByExpenseTypeAsync(string expenseTypeId, CancellationToken cancellationToken = default)
    {
        return context.Expenses.AnyAsync(e => e.ExpenseTypeId == expenseTypeId, cancellationToken);
    }

    public Task<int> CountAsync(string? search, string? expenseTypeId, CancellationToken cancellationToken = default)
    {
        string? cleanedSearch = search?.Trim().ToLowerInvariant();
        
        return context.Expenses
            .Where(e => (cleanedSearch == null || EF.Functions.ILike(e.Name, $"%{cleanedSearch}%")) &&
                       (expenseTypeId == null || e.ExpenseTypeId == expenseTypeId))
            .CountAsync(cancellationToken);
    }

    public void Insert(Expense expense)
    {
        context.Expenses.Add(expense);
    }

    public void Delete(Expense expense)
    {
        context.Expenses.Remove(expense);
    }
}
