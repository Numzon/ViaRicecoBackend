using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Portfolios.Domain.InvestedCashRecords;
using ViaRiceco.Modules.Portfolios.Infrastructure.Database;

namespace ViaRiceco.Modules.Portfolios.Infrastructure.InvestedCashRecords;

internal sealed class InvestedCashRecordRepository(PortfoliosDbContext context) : IInvestedCashRecordRepository
{
    public Task<InvestedCashRecord?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return context.InvestedCashRecords
            .FirstOrDefaultAsync(ic => ic.Id == id, cancellationToken);
    }

    public Task<InvestedCashRecord?> GetByMonthlyBudgetExpenseIdAsync(string monthlyBudgetExpenseId, CancellationToken cancellationToken = default)
    {
        return context.InvestedCashRecords
            .FirstOrDefaultAsync(ic => ic.MonthlyBudgetExpenseId == monthlyBudgetExpenseId, cancellationToken);
    }

    public Task<InvestedCashRecord?> GetByInvestmentStrategyAndMonthlyBudgetExpenseAsync(
        string investmentStrategyId, 
        string monthlyBudgetExpenseId, 
        CancellationToken cancellationToken = default)
    {
        return context.InvestedCashRecords
            .FirstOrDefaultAsync(ic => ic.InvestmentStrategyId == investmentStrategyId && 
                                     ic.MonthlyBudgetExpenseId == monthlyBudgetExpenseId, 
                               cancellationToken);
    }

    public async Task<IReadOnlyCollection<InvestedCashRecord>> GetByInvestmentStrategyAsync(
        string investmentStrategyId, 
        CancellationToken cancellationToken = default)
    {
        return await context.InvestedCashRecords
            .Where(ic => ic.InvestmentStrategyId == investmentStrategyId)
            .ToListAsync(cancellationToken);
    }

    public Task<decimal> GetTotalInvestedCashByInvestmentStrategyAsync(
        string investmentStrategyId, 
        CancellationToken cancellationToken = default)
    {
        return context.InvestedCashRecords
            .Where(ic => ic.InvestmentStrategyId == investmentStrategyId)
            .SumAsync(ic => ic.Amount, cancellationToken);
    }

    public void Insert(InvestedCashRecord investedCashRecord)
    {
        context.InvestedCashRecords.Add(investedCashRecord);
    }

    public void Delete(InvestedCashRecord investedCashRecord)
    {
        context.InvestedCashRecords.Remove(investedCashRecord);
    }
}

