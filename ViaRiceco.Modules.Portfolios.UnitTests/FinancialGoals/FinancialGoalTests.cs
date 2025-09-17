using FluentAssertions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Portfolios.Domain.FinancialGoals;
using ViaRiceco.Modules.Portfolios.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Portfolios.UnitTests.FinancialGoals;

public sealed class FinancialGoalTests : BaseTest
{
    [Fact]
    public void Create_Should_CreateFinancialGoalWithValidData()
    {
        // Arrange
        string name = Faker.Finance.AccountName();
        string? parentId = null;
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<FinancialGoal> result = FinancialGoal.Create(name, parentId, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        FinancialGoal goal = result.Value;
        goal.Should().NotBeNull();
        goal.Id.Should().StartWith("fg_");
        goal.Name.Should().Be(name);
        goal.ParentId.Should().Be(parentId);
        goal.CreatedAtUtc.Should().Be(createdAtUtc);
        goal.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_Should_CreateChildFinancialGoalWithParentId()
    {
        // Arrange
        string name = Faker.Finance.AccountName();
        string parentId = $"fg_{Guid.NewGuid()}";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<FinancialGoal> result = FinancialGoal.Create(name, parentId, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        FinancialGoal goal = result.Value;
        goal.ParentId.Should().Be(parentId);
    }

    [Fact]
    public void Create_Should_GenerateUniqueIds()
    {
        // Arrange
        string name = Faker.Finance.AccountName();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<FinancialGoal> result1 = FinancialGoal.Create(name, null, createdAtUtc);
        Result<FinancialGoal> result2 = FinancialGoal.Create(name, null, createdAtUtc);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.Id.Should().NotBe(result2.Value.Id);
        result1.Value.Id.Should().StartWith("fg_");
        result2.Value.Id.Should().StartWith("fg_");
    }

    [Fact]
    public void Create_Should_PublishFinancialGoalCreatedDomainEvent()
    {
        // Arrange
        string name = Faker.Finance.AccountName();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<FinancialGoal> result = FinancialGoal.Create(name, null, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        FinancialGoal goal = result.Value;
        FinancialGoalCreatedDomainEvent domainEvent = AssertDomainEventWasPublished<FinancialGoalCreatedDomainEvent>(goal);
        domainEvent.GoalId.Should().Be(goal.Id);
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
        Result<FinancialGoal> result = FinancialGoal.Create(invalidName!, null, createdAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Validation("FinancialGoal.InvalidName", "Financial goal name cannot be empty."));
    }

    [Fact]
    public void Update_Should_UpdateNameWhenDifferent()
    {
        // Arrange
        string originalName = "Retirement Fund";
        string newName = "Pension Savings";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        Result<FinancialGoal> createResult = FinancialGoal.Create(originalName, null, createdAtUtc);
        FinancialGoal goal = createResult.Value;

        // Act
        Result result = goal.Update(newName, null, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        goal.Name.Should().Be(newName);
        goal.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Update_Should_PublishFinancialGoalUpdatedDomainEvent_WhenNameChanges()
    {
        // Arrange
        string originalName = "Retirement Fund";
        string newName = "Pension Savings";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        Result<FinancialGoal> createResult = FinancialGoal.Create(originalName, null, createdAtUtc);
        FinancialGoal goal = createResult.Value;

        // Act
        Result result = goal.Update(newName, null, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        FinancialGoalUpdatedDomainEvent domainEvent = AssertDomainEventWasPublished<FinancialGoalUpdatedDomainEvent>(goal);
        domainEvent.GoalId.Should().Be(goal.Id);
        domainEvent.Name.Should().Be(newName);
        domainEvent.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Update_Should_NotUpdateOrPublishEvent_WhenNameIsSame()
    {
        // Arrange
        string name = "Retirement Fund";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        Result<FinancialGoal> createResult = FinancialGoal.Create(name, null, createdAtUtc);
        FinancialGoal goal = createResult.Value;

        // Act
        Result result = goal.Update(name, null, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        goal.DomainEvents.OfType<FinancialGoalUpdatedDomainEvent>().Should().BeEmpty();
        goal.UpdatedAtUtc.Should().BeNull(); // Should not update timestamp
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Update_Should_ReturnFailure_WhenNewNameIsInvalid(string? invalidName)
    {
        // Arrange
        string originalName = "Retirement Fund";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        Result<FinancialGoal> createResult = FinancialGoal.Create(originalName, null, createdAtUtc);
        FinancialGoal goal = createResult.Value;

        // Act
        Result result = goal.Update(invalidName!, null, updatedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.Validation("FinancialGoal.InvalidName", "Financial goal name cannot be empty."));
        goal.Name.Should().Be(originalName); // Should remain unchanged
    }

    [Theory]
    [InlineData("Emergency Fund")]
    [InlineData("House Down Payment")]
    [InlineData("Vacation Savings")]
    [InlineData("Education Fund")]
    [InlineData("Car Purchase")]
    public void Create_Should_HandleCommonFinancialGoalNames(string goalName)
    {
        // Arrange
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<FinancialGoal> result = FinancialGoal.Create(goalName, null, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(goalName);
        result.Value.Id.Should().StartWith("fg_");
    }

    [Fact]
    public void Create_Should_HandleLongNames()
    {
        // Arrange
        string longName = Faker.Lorem.Sentence(20); // Very long name
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<FinancialGoal> result = FinancialGoal.Create(longName, null, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(longName);
    }

    [Fact]
    public void Create_Should_HandleSpecialCharacters()
    {
        // Arrange
        string nameWithSpecialChars = "Retirement (401k) & Investment Portfolio - Goal #1";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<FinancialGoal> result = FinancialGoal.Create(nameWithSpecialChars, null, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(nameWithSpecialChars);
    }

    [Fact]
    public void Update_Should_HandleMultipleUpdates()
    {
        // Arrange
        string originalName = "Retirement Fund";
        string secondName = "Pension Savings";
        string finalName = "401k Portfolio";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime firstUpdateUtc = Faker.Date.RecentOffset().UtcDateTime;
        DateTime secondUpdateUtc = Faker.Date.FutureOffset().UtcDateTime;

        Result<FinancialGoal> createResult = FinancialGoal.Create(originalName, null, createdAtUtc);
        FinancialGoal goal = createResult.Value;

        // Act
        Result result1 = goal.Update(secondName, null, firstUpdateUtc);
        Result result2 = goal.Update(finalName, null, secondUpdateUtc);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        goal.Name.Should().Be(finalName);
        goal.UpdatedAtUtc.Should().Be(secondUpdateUtc);
        
        // Should have 3 domain events: 1 created + 2 updated
        goal.DomainEvents.Should().HaveCount(3);
        goal.DomainEvents.OfType<FinancialGoalCreatedDomainEvent>().Should().HaveCount(1);
        goal.DomainEvents.OfType<FinancialGoalUpdatedDomainEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void Create_Should_HandleUnicodeCharacters()
    {
        // Arrange
        string unicodeName = "Épargne Retraite (退職金) - صندوق التقاعد";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result<FinancialGoal> result = FinancialGoal.Create(unicodeName, null, createdAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(unicodeName);
    }

    [Fact]
    public void Update_Should_HandleCaseSensitiveChanges()
    {
        // Arrange
        string originalName = "retirement fund";
        string newName = "Retirement Fund"; // Same but different case
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        Result<FinancialGoal> createResult = FinancialGoal.Create(originalName, null, createdAtUtc);
        FinancialGoal goal = createResult.Value;

        // Act
        Result result = goal.Update(newName, null, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        goal.Name.Should().Be(newName);
        goal.UpdatedAtUtc.Should().Be(updatedAtUtc);
        
        // Should publish update event since strings are different
        FinancialGoalUpdatedDomainEvent domainEvent = AssertDomainEventWasPublished<FinancialGoalUpdatedDomainEvent>(goal);
        domainEvent.Name.Should().Be(newName);
    }
}
