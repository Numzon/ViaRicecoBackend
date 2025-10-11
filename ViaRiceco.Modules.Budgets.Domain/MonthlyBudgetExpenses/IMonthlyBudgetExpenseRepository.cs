namespace ViaRiceco.Modules.Budgets.Domain.MonthlyBudgetExpenses;

public interface IMonthlyBudgetExpenseRepository
{
    Task<MonthlyBudgetExpense?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MonthlyBudgetExpense>> GetByMonthlyBudgetIdAsync(string monthlyBudgetId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MonthlyBudgetExpense>> GetByExpenseIdAsync(string expenseId, CancellationToken cancellationToken = default); 
    Task<MonthlyBudgetExpense?> GetByMonthlyBudgetAndExpenseAsync(string monthlyBudgetId, string expenseId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForMonthlyBudgetAndExpenseAsync(string monthlyBudgetId, string expenseId, CancellationToken cancellationToken = default);
    void Insert(MonthlyBudgetExpense monthlyBudgetExpense);
    void Delete(MonthlyBudgetExpense monthlyBudgetExpense);
}
