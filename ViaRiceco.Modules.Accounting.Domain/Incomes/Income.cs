using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.Incomes;

public sealed class Income : Entity
{
    private Income()
    {
    }

    public decimal Value { get; private set; }
    public string SettlementPeriodId { get; private set; } = string.Empty;

    public static Income Create(decimal value, string settlementPeriodId, DateTime createdAtUtc)
    {
        var income = new Income
        {
            Id = $"i_{Guid.NewGuid()}",
            Value = value,
            SettlementPeriodId = settlementPeriodId,
            CreatedAtUtc = createdAtUtc
        };

        income.Raise(new IncomeProcessedDomainEvent(income.Id, income.SettlementPeriodId, createdAtUtc));

        return income;
    }

    public void Update(decimal value, DateTime updatedAtUtc)
    {
        if (Value == value)
        {
            return;
        }

        Value = value;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new IncomeUpdatedDomainEvent(Id, Value, updatedAtUtc));
    }
}
