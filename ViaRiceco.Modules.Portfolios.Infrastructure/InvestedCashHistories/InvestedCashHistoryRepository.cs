using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Portfolios.Domain.InvestedCashHistories;
using ViaRiceco.Modules.Portfolios.Infrastructure.Database;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.InvestedCashHistories;

internal sealed class InvestedCashHistoryRepository(PortfoliosDbContext context) : IInvestedCashHistoryRepository
{
    public Task<InvestedCashHistory?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return context.InvestedCash
            .FirstOrDefaultAsync(ic => ic.Id == id, cancellationToken);
    }

    public Task<InvestedCashHistory?> GetByInvestmentStrategyAndMonthlyBudgetExpenseAsync(
        string investmentStrategyId, 
        string monthlyBudgetExpenseId, 
        CancellationToken cancellationToken = default)
    {
        return context.InvestedCash
            .FirstOrDefaultAsync(ic => ic.InvestmentStrategyId == investmentStrategyId && 
                                     ic.MonthlyBudgetExpenseId == monthlyBudgetExpenseId, 
                               cancellationToken);
    }

    public async Task<IReadOnlyCollection<InvestedCashHistory>> GetByInvestmentStrategyAsync(
        string investmentStrategyId, 
        CancellationToken cancellationToken = default)
    {
        return await context.InvestedCash
            .Where(ic => ic.InvestmentStrategyId == investmentStrategyId)
            .ToListAsync(cancellationToken);
    }

    public Task<decimal> GetTotalInvestedCashByInvestmentStrategyAsync(
        string investmentStrategyId, 
        CancellationToken cancellationToken = default)
    {
        return context.InvestedCash
            .Where(ic => ic.InvestmentStrategyId == investmentStrategyId)
            .SumAsync(ic => ic.Amount, cancellationToken);
    }

    public void Insert(InvestedCashHistory investedCashHistory)
    {
        context.InvestedCash.Add(investedCashHistory);
    }

    public void Delete(InvestedCashHistory investedCashHistory)
    {
        context.InvestedCash.Remove(investedCashHistory);
    }
}
