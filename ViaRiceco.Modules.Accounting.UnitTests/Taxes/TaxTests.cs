using FluentAssertions;
using ViaRiceco.Modules.Accounting.Domain.Taxes;
using ViaRiceco.Modules.Accounting.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Accounting.UnitTests.Taxes;

public sealed class TaxTests : BaseTest
{
    [Fact]
    public void Create_Should_CreateTaxWithValidData()
    {
        // Arrange
        decimal value = Faker.Random.Decimal(1, 10000);
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string settlementPeriodId = $"sp_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var tax = Tax.Create(value, taxTypeId, settlementPeriodId, createdAtUtc);

        // Assert
        tax.Should().NotBeNull();
        tax.Id.Should().StartWith("t_");
        tax.Value.Should().Be(value);
        tax.TaxTypeId.Should().Be(taxTypeId);
        tax.SettlementPeriodId.Should().Be(settlementPeriodId);
        tax.CreatedAtUtc.Should().Be(createdAtUtc);
        tax.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_Should_GenerateUniqueIds()
    {
        // Arrange
        decimal value = Faker.Random.Decimal(1, 10000);
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string settlementPeriodId = $"sp_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var tax1 = Tax.Create(value, taxTypeId, settlementPeriodId, createdAtUtc);
        var tax2 = Tax.Create(value, taxTypeId, settlementPeriodId, createdAtUtc);

        // Assert
        tax1.Id.Should().NotBe(tax2.Id);
        tax1.Id.Should().StartWith("t_");
        tax2.Id.Should().StartWith("t_");
    }

    [Fact]
    public void Create_Should_PublishTaxProcessedDomainEvent()
    {
        // Arrange
        decimal value = Faker.Random.Decimal(1, 10000);
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string settlementPeriodId = $"sp_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var tax = Tax.Create(value, taxTypeId, settlementPeriodId, createdAtUtc);

        // Assert
        TaxProcessedDomainEvent domainEvent = AssertDomainEventWasPublished<TaxProcessedDomainEvent>(tax);
        domainEvent.TaxId.Should().Be(tax.Id);
        domainEvent.TaxTypeId.Should().Be(taxTypeId);
        domainEvent.SettlementPeriodId.Should().Be(settlementPeriodId);
        domainEvent.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public void Update_Should_UpdateValueWhenDifferent()
    {
        // Arrange
        decimal originalValue = Faker.Random.Decimal(1, 5000);
        decimal newValue = Faker.Random.Decimal(5001, 10000);
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string settlementPeriodId = $"sp_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var tax = Tax.Create(originalValue, taxTypeId, settlementPeriodId, createdAtUtc);

        // Act
        tax.Update(newValue, updatedAtUtc);

        // Assert
        tax.Value.Should().Be(newValue);
        tax.TaxTypeId.Should().Be(taxTypeId); // Should remain unchanged
        tax.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Update_Should_PublishTaxUpdatedDomainEvent_WhenValueChanges()
    {
        // Arrange
        decimal originalValue = Faker.Random.Decimal(1, 5000);
        decimal newValue = Faker.Random.Decimal(5001, 10000);
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string settlementPeriodId = $"sp_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var tax = Tax.Create(originalValue, taxTypeId, settlementPeriodId, createdAtUtc);

        // Act
        tax.Update(newValue, updatedAtUtc);

        // Assert
        TaxUpdatedDomainEvent domainEvent = AssertDomainEventWasPublished<TaxUpdatedDomainEvent>(tax);
        domainEvent.TaxId.Should().Be(tax.Id);
        domainEvent.Value.Should().Be(newValue);
        domainEvent.TaxTypeId.Should().Be(taxTypeId);
        domainEvent.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Update_Should_NotPublishTaxUpdatedDomainEvent_WhenValueIsSame()
    {
        // Arrange
        decimal value = Faker.Random.Decimal(1, 10000);
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string settlementPeriodId = $"sp_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var tax = Tax.Create(value, taxTypeId, settlementPeriodId, createdAtUtc);

        // Act
        tax.Update(value, updatedAtUtc);

        // Assert
        tax.DomainEvents.OfType<TaxUpdatedDomainEvent>().Should().BeEmpty();
        tax.UpdatedAtUtc.Should().BeNull(); // Should not update timestamp
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.75)]
    public void Create_Should_AcceptZeroAndNegativeValues(decimal value)
    {
        // Arrange
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string settlementPeriodId = $"sp_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var tax = Tax.Create(value, taxTypeId, settlementPeriodId, createdAtUtc);

        // Assert
        tax.Should().NotBeNull();
        tax.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_AcceptEmptyTaxTypeId(string taxTypeId)
    {
        // Note: This tests current behavior. In real application, 
        // you might want to validate that taxTypeId is not empty
        
        // Arrange
        decimal value = Faker.Random.Decimal(1, 10000);
        string settlementPeriodId = $"sp_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var tax = Tax.Create(value, taxTypeId, settlementPeriodId, createdAtUtc);

        // Assert
        tax.Should().NotBeNull();
        tax.TaxTypeId.Should().Be(taxTypeId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_AcceptEmptySettlementPeriodId(string settlementPeriodId)
    {
        // Note: This tests current behavior. In real application, 
        // you might want to validate that settlementPeriodId is not empty
        
        // Arrange
        decimal value = Faker.Random.Decimal(1, 10000);
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var tax = Tax.Create(value, taxTypeId, settlementPeriodId, createdAtUtc);

        // Assert
        tax.Should().NotBeNull();
        tax.SettlementPeriodId.Should().Be(settlementPeriodId);
    }

    [Fact]
    public void Update_Should_HandleMultipleUpdates()
    {
        // Arrange
        decimal originalValue = Faker.Random.Decimal(1, 3000);
        decimal secondValue = Faker.Random.Decimal(3001, 6000);
        decimal finalValue = Faker.Random.Decimal(6001, 10000);
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string settlementPeriodId = $"sp_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime firstUpdateUtc = Faker.Date.RecentOffset().UtcDateTime;
        DateTime secondUpdateUtc = Faker.Date.FutureOffset().UtcDateTime;

        var tax = Tax.Create(originalValue, taxTypeId, settlementPeriodId, createdAtUtc);

        // Act
        tax.Update(secondValue, firstUpdateUtc);
        tax.Update(finalValue, secondUpdateUtc);

        // Assert
        tax.Value.Should().Be(finalValue);
        tax.UpdatedAtUtc.Should().Be(secondUpdateUtc);
        
        // Should have 3 domain events: 1 created + 2 updated
        tax.DomainEvents.Should().HaveCount(3);
        tax.DomainEvents.OfType<TaxProcessedDomainEvent>().Should().HaveCount(1);
        tax.DomainEvents.OfType<TaxUpdatedDomainEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void TaxTypeId_Should_BeImmutable()
    {
        // Arrange
        decimal value = Faker.Random.Decimal(1, 10000);
        string originalTaxTypeId = $"tt_{Guid.NewGuid()}";
        string settlementPeriodId = $"sp_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var tax = Tax.Create(value, originalTaxTypeId, settlementPeriodId, createdAtUtc);

        // Act
        tax.Update(value + 100, updatedAtUtc);

        // Assert
        tax.TaxTypeId.Should().Be(originalTaxTypeId); // Should not change
    }

    [Fact]
    public void SettlementPeriodId_Should_BeImmutable()
    {
        // Arrange
        decimal value = Faker.Random.Decimal(1, 10000);
        string taxTypeId = $"tt_{Guid.NewGuid()}";
        string originalSettlementPeriodId = $"sp_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var tax = Tax.Create(value, taxTypeId, originalSettlementPeriodId, createdAtUtc);

        // Act
        tax.Update(value + 100, updatedAtUtc);

        // Assert
        tax.SettlementPeriodId.Should().Be(originalSettlementPeriodId); // Should not change
    }
}
