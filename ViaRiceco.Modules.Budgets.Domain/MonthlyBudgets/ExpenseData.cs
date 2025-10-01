using ViaRiceco.Modules.Budgets.Domain.Expenses;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;

namespace ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

public sealed record ExpenseData(Expense Expense, ExpenseType ExpenseType)
{
    public string ExpenseId => Expense.Id;
    public string ExpenseName => Expense.Name;
    public string ExpenseTypeId => ExpenseType.Id;
    public string ExpenseTypeName => ExpenseType.Name;
    
    public static ExpenseData From(Expense expense, ExpenseType expenseType)
    {
        return new ExpenseData(expense, expenseType);
    }
}
