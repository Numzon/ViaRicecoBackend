using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;

public sealed class ExpenseTypeUpdatedDomainEvent(string expenseTypeId, string name, DateTime updatedAtUtc) : DomainEvent
{
    public string ExpenseTypeId { get; init; } = expenseTypeId;
    public string Name { get; init; } = name;
    public DateTime UpdatedAtUtc { get; init; } = updatedAtUtc;  
}

