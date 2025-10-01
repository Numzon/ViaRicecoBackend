using ViaRiceco.Common.Domain.Models;

namespace ViaRiceco.Modules.Budgets.Domain.Banks;

public sealed class Bank : Entity
{
    private Bank()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public static Bank Create(string name, DateTime createdAtUtc)
    {
        var bank = new Bank
        {
            Id = $"b_{Guid.NewGuid()}",
            Name = name,
            CreatedAtUtc = createdAtUtc
        };

        bank.Raise(new BankCreatedDomainEvent(bank.Id, name, createdAtUtc));

        return bank;
    }

    public void Update(string name, DateTime updatedAtUtc)
    {
        if (Name == name)
        {
            return;
        }

        Name = name;
        UpdatedAtUtc = updatedAtUtc;

        Raise(new BankUpdatedDomainEvent(Id, name, updatedAtUtc));
    }
}
