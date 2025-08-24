using FluentAssertions;
using ViaRiceco.Modules.Accounting.Domain.Incomes;
using ViaRiceco.Modules.Accounting.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Accounting.UnitTests.Incomes;

public sealed class IncomeTests : BaseTest
{
    [Fact]
    public void Create_Should_CreateIncomeWithValidData()
    {
        // Arrange
        decimal value = Faker.Random.Decimal(1, 100000);
        string settlementPeriodId = $"sp_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var income = Income.Create(value, settlementPeriodId, createdAtUtc);

        // Assert
        income.Should().NotBeNull();
        income.Id.Should().StartWith("i_");
        income.Value.Should().Be(value);
        income.SettlementPeriodId.Should().Be(settlementPeriodId);
        income.CreatedAtUtc.Should().Be(createdAtUtc);
        income.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_Should_GenerateUniqueIds()
    {
        // Arrange
        decimal value = Faker.Random.Decimal(1, 100000);
        string settlementPeriodId = $"sp_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var income1 = Income.Create(value, settlementPeriodId, createdAtUtc);
        var income2 = Income.Create(value, settlementPeriodId, createdAtUtc);

        // Assert
        income1.Id.Should().NotBe(income2.Id);
        income1.Id.Should().StartWith("i_");
        income2.Id.Should().StartWith("i_");
    }

    [Fact]
    public void Create_Should_PublishIncomeProcessedDomainEvent()
    {
        // Arrange
        decimal value = Faker.Random.Decimal(1, 100000);
        string settlementPeriodId = $"sp_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var income = Income.Create(value, settlementPeriodId, createdAtUtc);

        // Assert
        IncomeProcessedDomainEvent domainEvent = AssertDomainEventWasPublished<IncomeProcessedDomainEvent>(income);
        domainEvent.IncomeId.Should().Be(income.Id);
        domainEvent.SettlementPeriodId.Should().Be(settlementPeriodId);
        domainEvent.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public void Update_Should_UpdateValueWhenDifferent()
    {
        // Arrange
        decimal originalValue = Faker.Random.Decimal(1, 50000);
        decimal newValue = Faker.Random.Decimal(50001, 100000);
        string settlementPeriodId = $"sp_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var income = Income.Create(originalValue, settlementPeriodId, createdAtUtc);

        // Act
        income.Update(newValue, updatedAtUtc);

        // Assert
        income.Value.Should().Be(newValue);
        income.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Update_Should_PublishIncomeUpdatedDomainEvent_WhenValueChanges()
    {
        // Arrange
        decimal originalValue = Faker.Random.Decimal(1, 50000);
        decimal newValue = Faker.Random.Decimal(50001, 100000);
        string settlementPeriodId = $"sp_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var income = Income.Create(originalValue, settlementPeriodId, createdAtUtc);

        // Act
        income.Update(newValue, updatedAtUtc);

        // Assert
        IncomeUpdatedDomainEvent domainEvent = AssertDomainEventWasPublished<IncomeUpdatedDomainEvent>(income);
        domainEvent.IncomeId.Should().Be(income.Id);
        domainEvent.Value.Should().Be(newValue);
        domainEvent.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Update_Should_NotPublishIncomeUpdatedDomainEvent_WhenValueIsSame()
    {
        // Arrange
        decimal value = Faker.Random.Decimal(1, 100000);
        string settlementPeriodId = $"sp_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var income = Income.Create(value, settlementPeriodId, createdAtUtc);

        // Act
        income.Update(value, updatedAtUtc);

        // Assert
        income.DomainEvents.OfType<IncomeUpdatedDomainEvent>().Should().BeEmpty();
        income.UpdatedAtUtc.Should().BeNull(); // Should not update timestamp
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Create_Should_AcceptZeroAndNegativeValues(decimal value)
    {
        // Arrange
        string settlementPeriodId = $"sp_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var income = Income.Create(value, settlementPeriodId, createdAtUtc);

        // Assert
        income.Should().NotBeNull();
        income.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Should_AcceptEmptySettlementPeriodId(string settlementPeriodId)
    {
        // Note: This tests current behavior. In real application, 
        // you might want to validate that settlementPeriodId is not empty
        
        // Arrange
        decimal value = Faker.Random.Decimal(1, 100000);
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var income = Income.Create(value, settlementPeriodId, createdAtUtc);

        // Assert
        income.Should().NotBeNull();
        income.SettlementPeriodId.Should().Be(settlementPeriodId);
    }

    [Fact]
    public void Update_Should_HandleMultipleUpdates()
    {
        // Arrange
        decimal originalValue = Faker.Random.Decimal(1, 30000);
        decimal secondValue = Faker.Random.Decimal(30001, 60000);
        decimal finalValue = Faker.Random.Decimal(60001, 100000);
        string settlementPeriodId = $"sp_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime firstUpdateUtc = Faker.Date.RecentOffset().UtcDateTime;
        DateTime secondUpdateUtc = Faker.Date.FutureOffset().UtcDateTime;

        var income = Income.Create(originalValue, settlementPeriodId, createdAtUtc);

        // Act
        income.Update(secondValue, firstUpdateUtc);
        income.Update(finalValue, secondUpdateUtc);

        // Assert
        income.Value.Should().Be(finalValue);
        income.UpdatedAtUtc.Should().Be(secondUpdateUtc);
        
        // Should have 3 domain events: 1 created + 2 updated
        income.DomainEvents.Should().HaveCount(3);
        income.DomainEvents.OfType<IncomeProcessedDomainEvent>().Should().HaveCount(1);
        income.DomainEvents.OfType<IncomeUpdatedDomainEvent>().Should().HaveCount(2);
    }
}
