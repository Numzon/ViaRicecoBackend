using FluentAssertions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;
using ViaRiceco.Modules.Budgets.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.UnitTests.ExpenseTypes;

public sealed class ExpenseTypeTests : BaseTest
{
    [Fact]
    public void Create_Should_CreateExpenseTypeWithValidData()
    {
        // Arrange
        string name = Faker.Commerce.Categories(1)[0];
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var expenseType = ExpenseType.Create(name, createdAtUtc);

        // Assert
        expenseType.Should().NotBeNull();
        expenseType.Id.Should().StartWith("et_");
        expenseType.Name.Should().Be(name);
        expenseType.IsSystemDefined.Should().BeFalse();
        expenseType.CreatedAtUtc.Should().Be(createdAtUtc);
        expenseType.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_Should_GenerateUniqueIds()
    {
        // Arrange
        string name = Faker.Commerce.Categories(1)[0];
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var expenseType1 = ExpenseType.Create(name, createdAtUtc);
        var expenseType2 = ExpenseType.Create(name, createdAtUtc);

        // Assert
        expenseType1.Id.Should().NotBe(expenseType2.Id);
        expenseType1.Id.Should().StartWith("et_");
        expenseType2.Id.Should().StartWith("et_");
    }

    [Fact]
    public void Create_Should_PublishExpenseTypeCreatedDomainEvent()
    {
        // Arrange
        string name = Faker.Commerce.Categories(1)[0];
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var expenseType = ExpenseType.Create(name, createdAtUtc);

        // Assert
        ExpenseTypeCreatedDomainEvent domainEvent = AssertDomainEventWasPublished<ExpenseTypeCreatedDomainEvent>(expenseType);
        domainEvent.ExpenseTypeId.Should().Be(expenseType.Id);
        domainEvent.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public void Update_Should_UpdateNameWhenDifferent()
    {
        // Arrange
        string originalName = "Travel";
        string newName = "Business Travel";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var expenseType = ExpenseType.Create(originalName, createdAtUtc);

        // Act
        Result result = expenseType.Update(newName, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        expenseType.Name.Should().Be(newName);
        expenseType.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Update_Should_PublishExpenseTypeUpdatedDomainEvent_WhenNameChanges()
    {
        // Arrange
        string originalName = "Travel";
        string newName = "Business Travel";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var expenseType = ExpenseType.Create(originalName, createdAtUtc);

        // Act
        expenseType.Update(newName, updatedAtUtc);

        // Assert
        ExpenseTypeUpdatedDomainEvent domainEvent = AssertDomainEventWasPublished<ExpenseTypeUpdatedDomainEvent>(expenseType);
        domainEvent.ExpenseTypeId.Should().Be(expenseType.Id);
        domainEvent.Name.Should().Be(newName);
        domainEvent.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Update_Should_ReturnSuccess_WhenNameIsSame()
    {
        // Arrange
        string name = "Travel";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var expenseType = ExpenseType.Create(name, createdAtUtc);

        // Act
        Result result = expenseType.Update(name, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        expenseType.DomainEvents.OfType<ExpenseTypeUpdatedDomainEvent>().Should().BeEmpty();
        expenseType.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Update_Should_ReturnFailure_WhenExpenseTypeIsSystemDefined()
    {
        // Arrange
        string name = "Investment";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Create a system-defined expense type using reflection to simulate database seeded data
        var expenseType = ExpenseType.Create(name, createdAtUtc);
        System.Reflection.PropertyInfo? isSystemDefinedField = typeof(ExpenseType).GetProperty("IsSystemDefined");
        isSystemDefinedField?.SetValue(expenseType, true);

        // Act
        Result result = expenseType.Update("Updated Investment", updatedAtUtc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ExpenseTypeErrors.CannotUpdateSystemDefined());
        expenseType.Name.Should().Be(name); // Should remain unchanged
        expenseType.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_Should_HandleLongNames()
    {
        // Arrange
        string longName = Faker.Lorem.Sentence(50);
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var expenseType = ExpenseType.Create(longName, createdAtUtc);

        // Assert
        expenseType.Should().NotBeNull();
        expenseType.Name.Should().Be(longName);
    }

    [Fact]
    public void Create_Should_HandleSpecialCharacters()
    {
        // Arrange
        string nameWithSpecialChars = "Travel & Entertainment (50%) - Business";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var expenseType = ExpenseType.Create(nameWithSpecialChars, createdAtUtc);

        // Assert
        expenseType.Should().NotBeNull();
        expenseType.Name.Should().Be(nameWithSpecialChars);
    }

    [Fact]
    public void Update_Should_HandleMultipleUpdates()
    {
        // Arrange
        string originalName = "Travel";
        string secondName = "Business Travel";
        string finalName = "Corporate Travel";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime firstUpdateUtc = Faker.Date.RecentOffset().UtcDateTime;
        DateTime secondUpdateUtc = Faker.Date.FutureOffset().UtcDateTime;

        var expenseType = ExpenseType.Create(originalName, createdAtUtc);

        // Act
        expenseType.Update(secondName, firstUpdateUtc);
        expenseType.Update(finalName, secondUpdateUtc);

        // Assert
        expenseType.Name.Should().Be(finalName);
        expenseType.UpdatedAtUtc.Should().Be(secondUpdateUtc);
        
        expenseType.DomainEvents.Should().HaveCount(3);
        expenseType.DomainEvents.OfType<ExpenseTypeCreatedDomainEvent>().Should().HaveCount(1);
        expenseType.DomainEvents.OfType<ExpenseTypeUpdatedDomainEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void Create_Should_HandleUnicodeCharacters()
    {
        // Arrange
        string unicodeName = "Frais de Transport (交通費) - نفقات النقل";
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var expenseType = ExpenseType.Create(unicodeName, createdAtUtc);

        // Assert
        expenseType.Should().NotBeNull();
        expenseType.Name.Should().Be(unicodeName);
    }

    [Theory]
    [InlineData("Travel")]
    [InlineData("Office Supplies")]
    [InlineData("Meals & Entertainment")]
    [InlineData("Professional Services")]
    [InlineData("Marketing")]
    public void Create_Should_HandleCommonExpenseTypeNames(string expenseTypeName)
    {
        // Arrange
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var expenseType = ExpenseType.Create(expenseTypeName, createdAtUtc);

        // Assert
        expenseType.Should().NotBeNull();
        expenseType.Name.Should().Be(expenseTypeName);
        expenseType.Id.Should().StartWith("et_");
    }

    [Fact]
    public void Update_Should_HandleCaseSensitiveChanges()
    {
        // Arrange
        string originalName = "travel expenses";
        string newName = "Travel Expenses";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var expenseType = ExpenseType.Create(originalName, createdAtUtc);

        // Act
        Result result = expenseType.Update(newName, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        expenseType.Name.Should().Be(newName);
        expenseType.UpdatedAtUtc.Should().Be(updatedAtUtc);
        
        ExpenseTypeUpdatedDomainEvent domainEvent = AssertDomainEventWasPublished<ExpenseTypeUpdatedDomainEvent>(expenseType);
        domainEvent.Name.Should().Be(newName);
    }
}
