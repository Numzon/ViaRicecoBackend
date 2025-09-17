using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Portfolios.Domain.FinancialGoals;
using ViaRiceco.Modules.Portfolios.Infrastructure.Database;
using System.Linq.Dynamic.Core;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.FinancialGoals;

internal sealed class FinancialGoalRepository(PortfoliosDbContext context) : IFinancialGoalRepository
{
    public Task<FinancialGoal?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return context.FinancialGoals.FirstOrDefaultAsync(fg => fg.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<FinancialGoal>> GetRootGoalsAsync(CancellationToken cancellationToken = default)
    {
        return await context.FinancialGoals
            .Where(fg => fg.ParentId == null)
            .OrderBy(fg => fg.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<FinancialGoal>> GetChildrenAsync(string parentId, CancellationToken cancellationToken = default)
    {
        return await context.FinancialGoals
            .Where(fg => fg.ParentId == parentId)
            .OrderBy(fg => fg.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<FinancialGoal>> GetPageAsync(string? search, string orderBy, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await context.FinancialGoals
            .Where(fg => string.IsNullOrWhiteSpace(search) || fg.Name.Contains(search))
            .OrderBy(orderBy)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return context.FinancialGoals.AnyAsync(fg => fg.Name == name, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string name, string excludeId, CancellationToken cancellationToken = default)
    {
        return context.FinancialGoals.AnyAsync(fg => fg.Name == name && fg.Id != excludeId, cancellationToken);
    }

    public Task<bool> HasChildrenAsync(string goalId, CancellationToken cancellationToken = default)
    {
        return context.FinancialGoals.AnyAsync(fg => fg.ParentId == goalId, cancellationToken);
    }

    public Task<int> CountAsync(string? search, CancellationToken cancellationToken = default)
    {
        return context.FinancialGoals
            .Where(fg => string.IsNullOrWhiteSpace(search) || fg.Name.Contains(search))
            .CountAsync(cancellationToken);
    }

    public void Insert(FinancialGoal financialGoal)
    {
        context.FinancialGoals.Add(financialGoal);
    }

    public void Delete(FinancialGoal financialGoal)
    {
        context.FinancialGoals.Remove(financialGoal);
    }
}
