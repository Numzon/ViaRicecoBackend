using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;

public sealed class ExpenseTypeCreatedDomainEvent(string expenseTypeId, DateTime createdAtUtc) : DomainEvent
{
    public string ExpenseTypeId { get; init; } = expenseTypeId;
    public DateTime CreatedAtUtc { get; init; } = createdAtUtc;  
}
