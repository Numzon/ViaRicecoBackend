namespace ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;

public sealed class MonthlyBudget
{
    //can be only created by integration event - after you save or update settlement period in accounting module 
    // when created copies Expenses to its own table MonthlyBudgetExpenses with their expense type name and id for expense type, also copies expense name and id for expenses
    // MonthlyBudgetExpense are created only from non-archived expenses 
    // MonthlyBudgetExpense have foreign keys to expense types and expenses
    // when there are existing MonthlyBudgetExpense with defined expense and expense type those two can't be deleted from expenses and expense-types tables, but can be set as archival,
    // archival expense and expense type can be set as not archival whenever user wants 
    // MonthlyBudgetExpense contains value - decimal, that can be nullable
    // after created it is set as draft with is an additional bool value
    // contains bussines rule - can be only save as final (draft boolean set as false) when all MonthlyBudgetExpenses have their value defined (not null)
    // MonthlyBudgetExpenses can be set to 0 and then MonthlyBudget can be set as final
    // when new expense is added it adds MonthlyBudgetExpense to the draft budget  
    // when expense is updated - domain event is consumed by monthy budget, it updates the name of MonthlyBudgetExpense but only when MonthlyBudget is set as draft
    // when Expense is archived then MonthlyBudgetExpense is removed from the draft budget
    // Expense type can be only archived all expenses that are using it are also archived 
}
