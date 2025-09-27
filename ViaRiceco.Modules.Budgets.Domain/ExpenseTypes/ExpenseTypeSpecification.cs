namespace ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;

public static class ExpenseTypeSpecification
{
    public sealed class Investment
    {
        public const string Id = "et_db610449-8a5f-47d0-be6a-ec26e4945375";   
        public const string Name = "Investment";
    }

    /// <summary>
    /// Determines whether the expense type can be deleted (not system-defined)
    /// </summary>
    public static bool CanBeDeleted(ExpenseType expenseType)
    {
        return !expenseType.IsSystemDefined;
    }
}
