using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Accounting.Domain.TaxTypes;

public sealed class TaxType : Entity
{
    private TaxType()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public static TaxType Create(string name, DateTime createdAtUtc)
    {
        var taxType = new TaxType
        {
            Id = $"tt_{Guid.NewGuid()}",
            Name = name,
            CreatedAtUtc = createdAtUtc
        };

        taxType.Raise(new TaxTypeProcessedDomainEvent(taxType.Id, createdAtUtc));

        return taxType;
    }

    public void Update(string name, DateTime updatedAtUtc)
    {
        if (Name == name)
        {
            return;
        }

        Name = name;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new TaxTypeUpdatedDomainEvent(Id, Name, updatedAtUtc));
    }
}
