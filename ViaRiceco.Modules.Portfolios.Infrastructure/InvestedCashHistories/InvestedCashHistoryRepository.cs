using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Portfolios.Domain.InvestedCashHistories;
using ViaRiceco.Modules.Portfolios.Infrastructure.Database;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.InvestedCashHistories;

internal sealed class InvestedCashHistoryRepository(PortfoliosDbContext context) : IInvestedCashHistoryRepository
{
    public Task<InvestedCashRecord?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return context.InvestedCashHistories
            .FirstOrDefaultAsync(ic => ic.Id == id, cancellationToken);
    }

    public Task<InvestedCashRecord?> GetByInvestmentStrategyAndMonthlyBudgetExpenseAsync(
        string investmentStrategyId, 
        string monthlyBudgetExpenseId, 
        CancellationToken cancellationToken = default)
    {
        return context.InvestedCashHistories
            .FirstOrDefaultAsync(ic => ic.InvestmentStrategyId == investmentStrategyId && 
                                     ic.MonthlyBudgetExpenseId == monthlyBudgetExpenseId, 
                               cancellationToken);
    }

    public async Task<IReadOnlyCollection<InvestedCashRecord>> GetByInvestmentStrategyAsync(
        string investmentStrategyId, 
        CancellationToken cancellationToken = default)
    {
        return await context.InvestedCashHistories
            .Where(ic => ic.InvestmentStrategyId == investmentStrategyId)
            .ToListAsync(cancellationToken);
    }

    public Task<decimal> GetTotalInvestedCashByInvestmentStrategyAsync(
        string investmentStrategyId, 
        CancellationToken cancellationToken = default)
    {
        return context.InvestedCashHistories
            .Where(ic => ic.InvestmentStrategyId == investmentStrategyId)
            .SumAsync(ic => ic.Amount, cancellationToken);
    }

    public void Insert(InvestedCashRecord investedCashRecord)
    {
        context.InvestedCashHistories.Add(investedCashRecord);
    }

    public void Delete(InvestedCashRecord investedCashRecord)
    {
        context.InvestedCashHistories.Remove(investedCashRecord);
    }
}

