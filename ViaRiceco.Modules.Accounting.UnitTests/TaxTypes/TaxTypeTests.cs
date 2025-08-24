using FluentAssertions;
using ViaRiceco.Modules.Accounting.Domain.TaxTypes;
using ViaRiceco.Modules.Accounting.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Accounting.UnitTests.TaxTypes;

public sealed class TaxTypeTests : BaseTest
{
    [Fact]
    public void Create_Should_CreateTaxTypeWithValidData()
    {
        // Arrange
        string name = Faker.Commerce.Categories(1)[0];
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var taxType = TaxType.Create(name, createdAtUtc);

        // Assert
        taxType.Should().NotBeNull();
        taxType.Id.Should().StartWith("tt_");
        taxType.Name.Should().Be(name);
        taxType.CreatedAtUtc.Should().Be(createdAtUtc);
        taxType.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_Should_GenerateUniqueIds()
    {
        // Arrange
        string name = Faker.Commerce.Categories(1)[0];
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var taxType1 = TaxType.Create(name, createdAtUtc);
        var taxType2 = TaxType.Create(name, createdAtUtc);

        // Assert
        taxType1.Id.Should().NotBe(taxType2.Id);
        taxType1.Id.Should().StartWith("tt_");
        taxType2.Id.Should().StartWith("tt_");
    }

    [Fact]
    public void Create_Should_PublishTaxTypeProcessedDomainEvent()
    {
        // Arrange
        string name = Faker.Commerce.Categories(1)[0];
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var taxType = TaxType.Create(name, createdAtUtc);

        // Assert
        TaxTypeProcessedDomainEvent domainEvent = AssertDomainEventWasPublished<TaxTypeProcessedDomainEvent>(taxType);
        domainEvent.TaxTypeId.Should().Be(taxType.Id);
        domainEvent.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public void Update_Should_UpdateNameWhenDifferent()
    {
        // Arrange
        string originalName = "Income Tax";
        string newName = "VAT Tax";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var taxType = TaxType.Create(originalName, createdAtUtc);

        // Act
        taxType.Update(newName, updatedAtUtc);

        // Assert
        taxType.Name.Should().Be(newName);
        taxType.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Update_Should_PublishTaxTypeUpdatedDomainEvent_WhenNameChanges()
    {
        // Arrange
        string originalName = "Income Tax";
        string newName = "VAT Tax";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var taxType = TaxType.Create(originalName, createdAtUtc);

        // Act
        taxType.Update(newName, updatedAtUtc);

        // Assert
        TaxTypeUpdatedDomainEvent domainEvent = AssertDomainEventWasPublished<TaxTypeUpdatedDomainEvent>(taxType);
        domainEvent.TaxTypeId.Should().Be(taxType.Id);
        domainEvent.Name.Should().Be(newName);
        domainEvent.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Update_Should_NotPublishTaxTypeUpdatedDomainEvent_WhenNameIsSame()
    {
        // Arrange
        string name = "Income Tax";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var taxType = TaxType.Create(name, createdAtUtc);

        // Act
        taxType.Update(name, updatedAtUtc);

        // Assert
        taxType.DomainEvents.OfType<TaxTypeUpdatedDomainEvent>().Should().BeEmpty();
        taxType.UpdatedAtUtc.Should().BeNull(); // Should not update timestamp
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_AcceptEmptyOrNullNames(string name)
    {
        // Note: This tests current behavior. In real application, 
        // you might want to validate that name is not empty
        
        // Arrange
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var taxType = TaxType.Create(name, createdAtUtc);

        // Assert
        taxType.Should().NotBeNull();
        taxType.Name.Should().Be(name ?? string.Empty); // Due to default initialization
    }

    [Fact]
    public void Create_Should_HandleLongNames()
    {
        // Arrange
        string longName = Faker.Lorem.Sentence(50); // Very long name
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var taxType = TaxType.Create(longName, createdAtUtc);

        // Assert
        taxType.Should().NotBeNull();
        taxType.Name.Should().Be(longName);
    }

    [Fact]
    public void Create_Should_HandleSpecialCharacters()
    {
        // Arrange
        string nameWithSpecialChars = "Tax & Fees (15%) - Special Rate";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var taxType = TaxType.Create(nameWithSpecialChars, createdAtUtc);

        // Assert
        taxType.Should().NotBeNull();
        taxType.Name.Should().Be(nameWithSpecialChars);
    }

    [Fact]
    public void Update_Should_HandleMultipleUpdates()
    {
        // Arrange
        string originalName = "Income Tax";
        string secondName = "VAT Tax";
        string finalName = "Corporate Tax";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime firstUpdateUtc = Faker.Date.RecentOffset().UtcDateTime;
        DateTime secondUpdateUtc = Faker.Date.FutureOffset().UtcDateTime;

        var taxType = TaxType.Create(originalName, createdAtUtc);

        // Act
        taxType.Update(secondName, firstUpdateUtc);
        taxType.Update(finalName, secondUpdateUtc);

        // Assert
        taxType.Name.Should().Be(finalName);
        taxType.UpdatedAtUtc.Should().Be(secondUpdateUtc);
        
        // Should have 3 domain events: 1 created + 2 updated
        taxType.DomainEvents.Should().HaveCount(3);
        taxType.DomainEvents.OfType<TaxTypeProcessedDomainEvent>().Should().HaveCount(1);
        taxType.DomainEvents.OfType<TaxTypeUpdatedDomainEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void Create_Should_HandleUnicodeCharacters()
    {
        // Arrange
        string unicodeName = "Impôt sur le Revenu (税金) - ضريبة الدخل";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var taxType = TaxType.Create(unicodeName, createdAtUtc);

        // Assert
        taxType.Should().NotBeNull();
        taxType.Name.Should().Be(unicodeName);
    }

    [Theory]
    [InlineData("Income Tax")]
    [InlineData("VAT")]
    [InlineData("Corporate Tax")]
    [InlineData("Property Tax")]
    [InlineData("Sales Tax")]
    public void Create_Should_HandleCommonTaxTypeNames(string taxTypeName)
    {
        // Arrange
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var taxType = TaxType.Create(taxTypeName, createdAtUtc);

        // Assert
        taxType.Should().NotBeNull();
        taxType.Name.Should().Be(taxTypeName);
        taxType.Id.Should().StartWith("tt_");
    }

    [Fact]
    public void Update_Should_HandleCaseSensitiveChanges()
    {
        // Arrange
        string originalName = "income tax";
        string newName = "Income Tax"; // Same but different case
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var taxType = TaxType.Create(originalName, createdAtUtc);

        // Act
        taxType.Update(newName, updatedAtUtc);

        // Assert
        taxType.Name.Should().Be(newName);
        taxType.UpdatedAtUtc.Should().Be(updatedAtUtc);
        
        // Should publish update event since strings are different
        TaxTypeUpdatedDomainEvent domainEvent = AssertDomainEventWasPublished<TaxTypeUpdatedDomainEvent>(taxType);
        domainEvent.Name.Should().Be(newName);
    }
}
