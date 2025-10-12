using FluentAssertions;
using ViaRiceco.Modules.Budgets.Domain.Banks;
using ViaRiceco.Modules.Budgets.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.UnitTests.Banks;

public sealed class BankTests : BaseTest
{
    [Fact]
    public void Create_Should_CreateBankWithValidData()
    {
        // Arrange
        string name = Faker.Company.CompanyName();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var bank = Bank.Create(name, createdAtUtc);

        // Assert
        bank.Should().NotBeNull();
        bank.Id.Should().StartWith("b_");
        bank.Name.Should().Be(name);
        bank.CreatedAtUtc.Should().Be(createdAtUtc);
        bank.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_Should_GenerateUniqueIds()
    {
        // Arrange
        string name = Faker.Company.CompanyName();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var bank1 = Bank.Create(name, createdAtUtc);
        var bank2 = Bank.Create(name, createdAtUtc);

        // Assert
        bank1.Id.Should().NotBe(bank2.Id);
        bank1.Id.Should().StartWith("b_");
        bank2.Id.Should().StartWith("b_");
    }

    [Fact]
    public void Create_Should_PublishBankCreatedDomainEvent()
    {
        // Arrange
        string name = Faker.Company.CompanyName();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var bank = Bank.Create(name, createdAtUtc);

        // Assert
        BankCreatedDomainEvent domainEvent = AssertDomainEventWasPublished<BankCreatedDomainEvent>(bank);
        domainEvent.BankId.Should().Be(bank.Id);
        domainEvent.Name.Should().Be(name);
        domainEvent.OccurredOnUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public void Update_Should_UpdateBankData()
    {
        // Arrange
        string originalName = "Original Bank";
        string newName = "Updated Bank";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var bank = Bank.Create(originalName, createdAtUtc);

        // Act
        bank.Update(newName, updatedAtUtc);

        // Assert
        bank.Name.Should().Be(newName);
        bank.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Update_Should_PublishBankUpdatedDomainEvent()
    {
        // Arrange
        string originalName = "Original Bank";
        string newName = "Updated Bank";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var bank = Bank.Create(originalName, createdAtUtc);

        // Act
        bank.Update(newName, updatedAtUtc);

        // Assert
        BankUpdatedDomainEvent domainEvent = AssertDomainEventWasPublished<BankUpdatedDomainEvent>(bank);
        domainEvent.BankId.Should().Be(bank.Id);
        domainEvent.Name.Should().Be(newName);
        domainEvent.OccurredOnUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Update_Should_NotUpdateOrPublishEvent_WhenNameIsTheSame()
    {
        // Arrange
        string name = "Same Bank Name";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var bank = Bank.Create(name, createdAtUtc);
        DateTime? originalUpdatedAt = bank.UpdatedAtUtc;

        // Act
        bank.Update(name, updatedAtUtc); // Same name

        // Assert
        bank.Name.Should().Be(name);
        bank.UpdatedAtUtc.Should().Be(originalUpdatedAt); // Should not be updated

        // Should only have the creation event, not the update event
        bank.DomainEvents.Should().HaveCount(1);
        bank.DomainEvents.Should().AllBeOfType<BankCreatedDomainEvent>();
    }

    [Theory]
    [InlineData("Chase Bank")]
    [InlineData("Bank of America")]
    [InlineData("Wells Fargo")]
    [InlineData("Citibank")]
    [InlineData("Goldman Sachs")]
    public void Create_Should_HandleCommonBankNames(string bankName)
    {
        // Arrange
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var bank = Bank.Create(bankName, createdAtUtc);

        // Assert
        bank.Should().NotBeNull();
        bank.Name.Should().Be(bankName);
        bank.Id.Should().StartWith("b_");
        bank.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public void Create_Should_HandleUnicodeCharacters()
    {
        // Arrange
        string unicodeName = "Société Générale 中国银行 البنك الأهلي";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var bank = Bank.Create(unicodeName, createdAtUtc);

        // Assert
        bank.Should().NotBeNull();
        bank.Name.Should().Be(unicodeName);
    }

    [Fact]
    public void Update_Should_HandleMultipleUpdates()
    {
        // Arrange
        string originalName = "Original Bank";
        string firstUpdate = "First Update";
        string secondUpdate = "Second Update";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime firstUpdatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        DateTime secondUpdatedAtUtc = firstUpdatedAtUtc.AddMinutes(10);

        var bank = Bank.Create(originalName, createdAtUtc);

        // Act
        bank.Update(firstUpdate, firstUpdatedAtUtc);
        bank.Update(secondUpdate, secondUpdatedAtUtc);

        // Assert
        bank.Name.Should().Be(secondUpdate);
        bank.UpdatedAtUtc.Should().Be(secondUpdatedAtUtc);
        
        // Should have 3 events: Create + 2 Updates
        bank.DomainEvents.Should().HaveCount(3);
        bank.DomainEvents.Should().ContainSingle(e => e is BankCreatedDomainEvent);
        bank.DomainEvents.OfType<BankUpdatedDomainEvent>().Should().HaveCount(2);
    }
}
