using FluentAssertions;
using ViaRiceco.Modules.Budgets.Domain.Expenses;
using ViaRiceco.Modules.Budgets.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.UnitTests.Expenses;

public sealed class ExpenseTests : BaseTest
{
    [Fact]
    public void Create_Should_CreateExpenseWithValidData()
    {
        // Arrange
        string name = Faker.Commerce.ProductName();
        string expenseTypeId = "et_" + Faker.Random.Guid();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var expense = Expense.Create(name, expenseTypeId, createdAtUtc);

        // Assert
        expense.Should().NotBeNull();
        expense.Id.Should().StartWith("e_");
        expense.Name.Should().Be(name);
        expense.ExpenseTypeId.Should().Be(expenseTypeId);
        expense.CreatedAtUtc.Should().Be(createdAtUtc);
        expense.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_Should_GenerateUniqueIds()
    {
        // Arrange
        string name = Faker.Commerce.ProductName();
        string expenseTypeId = "et_" + Faker.Random.Guid();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var expense1 = Expense.Create(name, expenseTypeId, createdAtUtc);
        var expense2 = Expense.Create(name, expenseTypeId, createdAtUtc);

        // Assert
        expense1.Id.Should().NotBe(expense2.Id);
        expense1.Id.Should().StartWith("e_");
        expense2.Id.Should().StartWith("e_");
    }

    [Fact]
    public void Create_Should_PublishExpenseCreatedDomainEvent()
    {
        // Arrange
        string name = Faker.Commerce.ProductName();
        string expenseTypeId = "et_" + Faker.Random.Guid();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var expense = Expense.Create(name, expenseTypeId, createdAtUtc);

        // Assert
        ExpenseCreatedDomainEvent domainEvent = AssertDomainEventWasPublished<ExpenseCreatedDomainEvent>(expense);
        domainEvent.ExpenseId.Should().Be(expense.Id);
        domainEvent.Name.Should().Be(name);
        domainEvent.ExpenseTypeId.Should().Be(expenseTypeId);
        domainEvent.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public void CreateFromIntegrationEvent_Should_CreateExpenseWithProvidedId()
    {
        // Arrange
        string name = Faker.Commerce.ProductName();
        string expenseTypeId = "et_" + Faker.Random.Guid();
        string investmentStrategyId = "is_" + Faker.Random.Guid();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var expense = Expense.CreateFromIntegrationEvent(name, expenseTypeId, investmentStrategyId, createdAtUtc);

        // Assert
        expense.Should().NotBeNull();
        expense.Name.Should().Be(name);
        expense.ExpenseTypeId.Should().Be(expenseTypeId);
        expense.CreatedAtUtc.Should().Be(createdAtUtc);
        expense.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void CreateFromIntegrationEvent_Should_PublishExpenseCreatedFromIntegrationEventDomainEvent()
    {
        // Arrange
        string name = Faker.Commerce.ProductName();
        string investmentStrategyId = "is_" + Faker.Random.Guid();
        string expenseTypeId = "et_" + Faker.Random.Guid();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var expense = Expense.CreateFromIntegrationEvent(name, expenseTypeId, investmentStrategyId, createdAtUtc);

        // Assert
        ExpenseCreatedFromIntegrationEventDomainEvent domainEvent =
            AssertDomainEventWasPublished<ExpenseCreatedFromIntegrationEventDomainEvent>(expense);
        domainEvent.Name.Should().Be(name);
        domainEvent.ExpenseTypeId.Should().Be(expenseTypeId);
        domainEvent.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public void Update_Should_UpdateExpenseData()
    {
        // Arrange
        string originalName = "Original Expense";
        string originalExpenseTypeId = "et_" + Faker.Random.Guid();
        string newName = "Updated Expense";
        string newExpenseTypeId = "et_" + Faker.Random.Guid();
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var expense = Expense.Create(originalName, originalExpenseTypeId, createdAtUtc);

        // Act
        expense.Update(newName, newExpenseTypeId, updatedAtUtc);

        // Assert
        expense.Name.Should().Be(newName);
        expense.ExpenseTypeId.Should().Be(newExpenseTypeId);
        expense.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Update_Should_PublishExpenseUpdatedDomainEvent()
    {
        // Arrange
        string originalName = "Original Expense";
        string originalExpenseTypeId = "et_" + Faker.Random.Guid();
        string newName = "Updated Expense";
        string newExpenseTypeId = "et_" + Faker.Random.Guid();
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var expense = Expense.Create(originalName, originalExpenseTypeId, createdAtUtc);

        // Act
        expense.Update(newName, newExpenseTypeId, updatedAtUtc);

        // Assert
        ExpenseUpdatedDomainEvent domainEvent = AssertDomainEventWasPublished<ExpenseUpdatedDomainEvent>(expense);
        domainEvent.ExpenseId.Should().Be(expense.Id);
        domainEvent.Name.Should().Be(newName);
        domainEvent.ExpenseTypeId.Should().Be(newExpenseTypeId);
        domainEvent.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Update_Should_UpdateOnlyChangedFields()
    {
        // Arrange
        string name = "Test Expense";
        string originalExpenseTypeId = "et_" + Faker.Random.Guid();
        string newExpenseTypeId = "et_" + Faker.Random.Guid();
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        var expense = Expense.Create(name, originalExpenseTypeId, createdAtUtc);

        // Act - update only expense type, keep same name
        expense.Update(name, newExpenseTypeId, updatedAtUtc);

        // Assert
        expense.Name.Should().Be(name);
        expense.ExpenseTypeId.Should().Be(newExpenseTypeId);
        expense.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void Create_Should_HandleLongNames()
    {
        // Arrange
        string longName = Faker.Lorem.Sentence(50);
        string expenseTypeId = "et_" + Faker.Random.Guid();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var expense = Expense.Create(longName, expenseTypeId, createdAtUtc);

        // Assert
        expense.Should().NotBeNull();
        expense.Name.Should().Be(longName);
    }

    [Fact]
    public void Create_Should_HandleSpecialCharacters()
    {
        // Arrange
        string nameWithSpecialChars = "Hotel @ Paris (3 nights) - €150/night";
        string expenseTypeId = "et_" + Faker.Random.Guid();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var expense = Expense.Create(nameWithSpecialChars, expenseTypeId, createdAtUtc);

        // Assert
        expense.Should().NotBeNull();
        expense.Name.Should().Be(nameWithSpecialChars);
    }

    [Fact]
    public void Update_Should_HandleMultipleUpdates()
    {
        // Arrange
        string originalName = "Original Expense";
        string expenseTypeId = "et_" + Faker.Random.Guid();
        string secondName = "Second Update";
        string finalName = "Final Update";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime firstUpdateUtc = Faker.Date.RecentOffset().UtcDateTime;
        DateTime secondUpdateUtc = Faker.Date.FutureOffset().UtcDateTime;

        var expense = Expense.Create(originalName, expenseTypeId, createdAtUtc);

        // Act
        expense.Update(secondName, expenseTypeId, firstUpdateUtc);
        expense.Update(finalName, expenseTypeId, secondUpdateUtc);

        // Assert
        expense.Name.Should().Be(finalName);
        expense.UpdatedAtUtc.Should().Be(secondUpdateUtc);

        expense.DomainEvents.Should().HaveCount(3);
        expense.DomainEvents.OfType<ExpenseCreatedDomainEvent>().Should().HaveCount(1);
        expense.DomainEvents.OfType<ExpenseUpdatedDomainEvent>().Should().HaveCount(2);
    }

    [Fact]
    public void Create_Should_HandleUnicodeCharacters()
    {
        // Arrange
        string unicodeName = "Hôtel à Paris (パリのホテル) - فندق في باريس";
        string expenseTypeId = "et_" + Faker.Random.Guid();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var expense = Expense.Create(unicodeName, expenseTypeId, createdAtUtc);

        // Assert
        expense.Should().NotBeNull();
        expense.Name.Should().Be(unicodeName);
    }

    [Theory]
    [InlineData("Flight to London")]
    [InlineData("Hotel Accommodation")]
    [InlineData("Business Lunch")]
    [InlineData("Office Supplies")]
    [InlineData("Software License")]
    public void Create_Should_HandleCommonExpenseNames(string expenseName)
    {
        // Arrange
        string expenseTypeId = "et_" + Faker.Random.Guid();
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var expense = Expense.Create(expenseName, expenseTypeId, createdAtUtc);

        // Assert
        expense.Should().NotBeNull();
        expense.Name.Should().Be(expenseName);
        expense.Id.Should().StartWith("e_");
        expense.ExpenseTypeId.Should().Be(expenseTypeId);
    }
}
