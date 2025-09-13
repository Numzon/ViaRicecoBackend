using FluentAssertions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Domain.InvestmentStrategyTypes;
using ViaRiceco.Modules.Portfolios.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Portfolios.UnitTests.InvestmentStrategyTypes;

public sealed class InvestmentStrategyTypeTests : BaseTest
{
    [Fact]
    public void Create_Should_CreateInvestmentStrategyTypeWithValidData()
    {
        // Arrange
        string name = Faker.Finance.AccountName();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<InvestmentStrategyType> result = InvestmentStrategyType.Create(name, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        InvestmentStrategyType strategyType = result.Value;
        strategyType.Should().NotBeNull();
        strategyType.Id.Should().StartWith("ist_");
        strategyType.Name.Should().Be(name);
        strategyType.CreatedAtUtc.Should().Be(createdAtUtc);
        strategyType.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_Should_GenerateUniqueIds()
    {
        // Arrange
        string name = Faker.Finance.AccountName();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<InvestmentStrategyType> result1 = InvestmentStrategyType.Create(name, createdAtUtc);
        Result<InvestmentStrategyType> result2 = InvestmentStrategyType.Create(name, createdAtUtc);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.Id.Should().NotBe(result2.Value.Id);
        result1.Value.Id.Should().StartWith("ist_");
        result2.Value.Id.Should().StartWith("ist_");
    }

    [Fact]
    public void Create_Should_PublishInvestmentStrategyTypeCreatedDomainEvent()
    {
        // Arrange
        string name = Faker.Finance.AccountName();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<InvestmentStrategyType> result = InvestmentStrategyType.Create(name, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        InvestmentStrategyType strategyType = result.Value;
        InvestmentStrategyTypeCreatedDomainEvent domainEvent = AssertDomainEventWasPublished<InvestmentStrategyTypeCreatedDomainEvent>(strategyType);
        domainEvent.StrategyTypeId.Should().Be(strategyType.Id);
        domainEvent.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_Should_ReturnFailure_WhenNameIsInvalid(string? invalidName)
    {
        // Arrange
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<InvestmentStrategyType> result = InvestmentStrategyType.Create(invalidName!, createdAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Validation("InvestmentStrategyType.InvalidName", "Investment strategy type name cannot be empty."));
    }

    [Fact]
    public void Update_Should_UpdateNameWhenDifferent()
    {
        // Arrange
        string originalName = "Growth Strategy";
        string newName = "Aggressive Growth Strategy";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        Result<InvestmentStrategyType> createResult = InvestmentStrategyType.Create(originalName, createdAtUtc);
        InvestmentStrategyType strategyType = createResult.Value;

        // Act
        Result result = strategyType.Update(newName, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        strategyType.Name.Should().Be(newName);
        strategyType.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Update_Should_PublishInvestmentStrategyTypeUpdatedDomainEvent_WhenNameChanges()
    {
        // Arrange
        string originalName = "Growth Strategy";
        string newName = "Aggressive Growth Strategy";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        Result<InvestmentStrategyType> createResult = InvestmentStrategyType.Create(originalName, createdAtUtc);
        InvestmentStrategyType strategyType = createResult.Value;

        // Act
        Result result = strategyType.Update(newName, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        InvestmentStrategyTypeUpdatedDomainEvent domainEvent = AssertDomainEventWasPublished<InvestmentStrategyTypeUpdatedDomainEvent>(strategyType);
        domainEvent.StrategyTypeId.Should().Be(strategyType.Id);
        domainEvent.Name.Should().Be(newName);
        domainEvent.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Update_Should_NotUpdateOrPublishEvent_WhenNameIsSame()
    {
        // Arrange
        string name = "Growth Strategy";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        Result<InvestmentStrategyType> createResult = InvestmentStrategyType.Create(name, createdAtUtc);
        InvestmentStrategyType strategyType = createResult.Value;

        // Act
        Result result = strategyType.Update(name, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        strategyType.DomainEvents.OfType<InvestmentStrategyTypeUpdatedDomainEvent>().Should().BeEmpty();
        strategyType.UpdatedAtUtc.Should().BeNull(); // Should not update timestamp
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Update_Should_ReturnFailure_WhenNewNameIsInvalid(string? invalidName)
    {
        // Arrange
        string originalName = "Growth Strategy";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        Result<InvestmentStrategyType> createResult = InvestmentStrategyType.Create(originalName, createdAtUtc);
        InvestmentStrategyType strategyType = createResult.Value;

        // Act
        Result result = strategyType.Update(invalidName!, updatedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Validation("InvestmentStrategyType.InvalidName", "Investment strategy type name cannot be empty."));
        strategyType.Name.Should().Be(originalName); // Should remain unchanged
    }

    [Theory]
    [InlineData("Conservative")]
    [InlineData("Moderate")]
    [InlineData("Growth")]
    [InlineData("Aggressive")]
    [InlineData("Balanced")]
    public void Create_Should_HandleCommonStrategyTypeNames(string strategyTypeName)
    {
        // Arrange
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<InvestmentStrategyType> result = InvestmentStrategyType.Create(strategyTypeName, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(strategyTypeName);
        result.Value.Id.Should().StartWith("ist_");
    }

    [Fact]
    public void Create_Should_HandleLongNames()
    {
        // Arrange
        string longName = Faker.Lorem.Sentence(15); // Very long name
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<InvestmentStrategyType> result = InvestmentStrategyType.Create(longName, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(longName);
    }

    [Fact]
    public void Create_Should_HandleSpecialCharacters()
    {
        // Arrange
        string nameWithSpecialChars = "Growth & Income Strategy (5-10% Bonds) - Portfolio #1";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<InvestmentStrategyType> result = InvestmentStrategyType.Create(nameWithSpecialChars, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(nameWithSpecialChars);
    }

    [Fact]
    public void Update_Should_HandleMultipleUpdates()
    {
        // Arrange
        string originalName = "Growth Strategy";
        string secondName = "Aggressive Growth";
        string finalName = "Ultra Aggressive Growth";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime firstUpdateUtc = Faker.Date.RecentOffset().UtcDateTime;
        DateTime secondUpdateUtc = Faker.Date.FutureOffset().UtcDateTime;

        Result<InvestmentStrategyType> createResult = InvestmentStrategyType.Create(originalName, createdAtUtc);
        InvestmentStrategyType strategyType = createResult.Value;

        // Act
        Result result1 = strategyType.Update(secondName, firstUpdateUtc);
        Result result2 = strategyType.Update(finalName, secondUpdateUtc);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        strategyType.Name.Should().Be(finalName);
        strategyType.UpdatedAtUtc.Should().Be(secondUpdateUtc);
        
        // Should have 3 domain events: 1 created + 2 updated
        strategyType.DomainEvents.Should().HaveCount(3);
        strategyType.DomainEvents.OfType<InvestmentStrategyTypeCreatedDomainEvent>().Should().HaveCount(1);
        strategyType.DomainEvents.OfType<InvestmentStrategyTypeUpdatedDomainEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void Create_Should_HandleUnicodeCharacters()
    {
        // Arrange
        string unicodeName = "Stratégie de Croissance (成長戦略) - استراتيجية النمو";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<InvestmentStrategyType> result = InvestmentStrategyType.Create(unicodeName, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(unicodeName);
    }

    [Fact]
    public void Update_Should_HandleCaseSensitiveChanges()
    {
        // Arrange
        string originalName = "growth strategy";
        string newName = "Growth Strategy"; // Same but different case
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        Result<InvestmentStrategyType> createResult = InvestmentStrategyType.Create(originalName, createdAtUtc);
        InvestmentStrategyType strategyType = createResult.Value;

        // Act
        Result result = strategyType.Update(newName, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        strategyType.Name.Should().Be(newName);
        strategyType.UpdatedAtUtc.Should().Be(updatedAtUtc);
        
        // Should publish update event since strings are different
        InvestmentStrategyTypeUpdatedDomainEvent domainEvent = AssertDomainEventWasPublished<InvestmentStrategyTypeUpdatedDomainEvent>(strategyType);
        domainEvent.Name.Should().Be(newName);
    }
}
