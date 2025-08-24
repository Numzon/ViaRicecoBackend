using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.Taxes;

public sealed class Tax : Entity
{
    private Tax()
    {
    }

    public decimal Value { get; private set; }
    public string TaxTypeId { get; private set; } = string.Empty;
    public string SettlementPeriodId { get; private set; } = string.Empty;

    public static Tax Create(decimal value, string taxTypeId, string settlementPeriodId, DateTime createdAtUtc)
    {
        var tax = new Tax
        {
            Id = $"t_{Guid.NewGuid()}",
            Value = value,
            TaxTypeId = taxTypeId,
            SettlementPeriodId = settlementPeriodId,
            CreatedAtUtc = createdAtUtc
        };

        tax.Raise(new TaxProcessedDomainEvent(tax.Id, tax.TaxTypeId, tax.SettlementPeriodId, createdAtUtc));

        return tax;
    }

    public void Update(decimal value, DateTime updatedAtUtc)
    {
        if (Value == value)
        {
            return;
        }

        Value = value;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new TaxUpdatedDomainEvent(Id, Value, TaxTypeId, updatedAtUtc));
    }
}
