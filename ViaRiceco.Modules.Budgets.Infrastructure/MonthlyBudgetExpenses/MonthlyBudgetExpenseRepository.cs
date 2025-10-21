using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgetExpenses;
using ViaRiceco.Modules.Budgets.Infrastructure.Database;

namespace ViaRiceco.Modules.Budgets.Infrastructure.MonthlyBudgetExpenses;

internal sealed class MonthlyBudgetExpenseRepository(BudgetsDbContext context) : IMonthlyBudgetExpenseRepository
{
    public async Task<MonthlyBudgetExpense?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return await context.MonthlyBudgetExpenses
            .FirstOrDefaultAsync(mbe => mbe.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<MonthlyBudgetExpense>> GetByMonthlyBudgetIdAsync(string monthlyBudgetId, CancellationToken cancellationToken = default)
    {
        return await context.MonthlyBudgetExpenses
            .Where(mbe => mbe.MonthlyBudgetId == monthlyBudgetId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<MonthlyBudgetExpense>> GetByExpenseIdAsync(string expenseId, CancellationToken cancellationToken = default)
    {
        return await context.MonthlyBudgetExpenses
            .Where(mbe => mbe.ExpenseId == expenseId)
            .ToListAsync(cancellationToken);
    }

    public async Task<MonthlyBudgetExpense?> GetByMonthlyBudgetAndExpenseAsync(string monthlyBudgetId, string expenseId, CancellationToken cancellationToken = default)
    {
        return await context.MonthlyBudgetExpenses
            .FirstOrDefaultAsync(mbe => mbe.MonthlyBudgetId == monthlyBudgetId && mbe.ExpenseId == expenseId, cancellationToken);
    }

    public async Task<bool> ExistsForMonthlyBudgetAndExpenseAsync(string monthlyBudgetId, string expenseId, CancellationToken cancellationToken = default)
    {
        return await context.MonthlyBudgetExpenses
            .AnyAsync(mbe => mbe.MonthlyBudgetId == monthlyBudgetId && mbe.ExpenseId == expenseId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<MonthlyBudgetExpenseWithExpense>> GetByIdsWithInvestmentExpensesAsync(
        IReadOnlyCollection<string> monthlyBudgetExpenseIds,
        CancellationToken cancellationToken = default)
    {
        return await context.MonthlyBudgetExpenses
            .Where(mbe => monthlyBudgetExpenseIds.Contains(mbe.Id))
            .Join(context.Expenses,
                mbe => mbe.ExpenseId,
                expense => expense.Id,
                (mbe, expense) => new { MonthlyBudgetExpense = mbe, Expense = expense })
            .Where(joined => joined.Expense.ExpenseTypeId == ExpenseTypeSpecification.Investment.Id)
            .Select(joined => new MonthlyBudgetExpenseWithExpense(
                joined.MonthlyBudgetExpense.Id,
                joined.MonthlyBudgetExpense.Value,
                joined.Expense.Id,
                joined.Expense.ExpenseTypeId,
                joined.Expense.InvestmentStrategyId))
            .ToListAsync(cancellationToken);
    }

    public void Insert(MonthlyBudgetExpense monthlyBudgetExpense)
    {
        context.MonthlyBudgetExpenses.Add(monthlyBudgetExpense);
    }

    public void Delete(MonthlyBudgetExpense monthlyBudgetExpense)
    {
        context.MonthlyBudgetExpenses.Remove(monthlyBudgetExpense);
    }
}
