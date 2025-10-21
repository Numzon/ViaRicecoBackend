using FluentAssertions;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgetExpenses;
using ViaRiceco.Modules.Budgets.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.UnitTests.MonthlyBudgetExpenses;

public sealed class MonthlyBudgetExpenseTests : BaseTest
{
    [Fact]
    public void Create_Should_CreateMonthlyBudgetExpenseWithValidData()
    {
        // Arrange
        string monthlyBudgetId = "mb_" + Faker.Random.Guid();
        string expenseId = "e_" + Faker.Random.Guid();
        string expenseName = Faker.Commerce.ProductName();
        string expenseTypeId = "et_" + Faker.Random.Guid();
        string expenseTypeName = Faker.Commerce.Categories(1)[0];
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var expense = MonthlyBudgetExpense.Create(
            monthlyBudgetId,
            expenseId,
            expenseName,
            expenseTypeId,
            expenseTypeName,
            createdAtUtc);

        // Assert
        expense.Should().NotBeNull();
        expense.Id.Should().StartWith("mbe_");
        expense.MonthlyBudgetId.Should().Be(monthlyBudgetId);
        expense.ExpenseId.Should().Be(expenseId);
        expense.ExpenseName.Should().Be(expenseName);
        expense.ExpenseTypeId.Should().Be(expenseTypeId);
        expense.ExpenseTypeName.Should().Be(expenseTypeName);
        expense.Value.Should().BeNull(); // Starts as null
        expense.CreatedAtUtc.Should().Be(createdAtUtc);
        expense.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_Should_GenerateUniqueIds()
    {
        // Arrange
        string monthlyBudgetId = "mb_" + Faker.Random.Guid();
        string expenseId = "e_" + Faker.Random.Guid();
        string expenseName = Faker.Commerce.ProductName();
        string expenseTypeId = "et_" + Faker.Random.Guid();
        string expenseTypeName = Faker.Commerce.Categories(1)[0];
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var expense1 = MonthlyBudgetExpense.Create(
            monthlyBudgetId, expenseId, expenseName, expenseTypeId, expenseTypeName, createdAtUtc);
        var expense2 = MonthlyBudgetExpense.Create(
            monthlyBudgetId, expenseId, expenseName, expenseTypeId, expenseTypeName, createdAtUtc);

        // Assert
        expense1.Id.Should().NotBe(expense2.Id);
        expense1.Id.Should().StartWith("mbe_");
        expense2.Id.Should().StartWith("mbe_");
    }

    [Fact]
    public void Create_Should_PublishMonthlyBudgetExpenseCreatedDomainEvent()
    {
        // Arrange
        string monthlyBudgetId = "mb_" + Faker.Random.Guid();
        string expenseId = "e_" + Faker.Random.Guid();
        string expenseName = Faker.Commerce.ProductName();
        string expenseTypeId = "et_" + Faker.Random.Guid();
        string expenseTypeName = Faker.Commerce.Categories(1)[0];
        DateTime createdAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        var expense = MonthlyBudgetExpense.Create(
            monthlyBudgetId, expenseId, expenseName, expenseTypeId, expenseTypeName, createdAtUtc);

        // Assert
        MonthlyBudgetExpenseCreatedDomainEvent domainEvent = 
            AssertDomainEventWasPublished<MonthlyBudgetExpenseCreatedDomainEvent>(expense);
        domainEvent.MonthlyBudgetExpenseId.Should().Be(expense.Id);
        domainEvent.MonthlyBudgetId.Should().Be(monthlyBudgetId);
        domainEvent.ExpenseId.Should().Be(expenseId);
        domainEvent.ExpenseName.Should().Be(expenseName);
        domainEvent.ExpenseTypeId.Should().Be(expenseTypeId);
        domainEvent.ExpenseTypeName.Should().Be(expenseTypeName);
        domainEvent.OccurredOnUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public void UpdateExpenseName_Should_UpdateExpenseNameData()
    {
        // Arrange
        string originalExpenseName = "Original Expense";
        string newExpenseName = "Updated Expense";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        MonthlyBudgetExpense expense = CreateMonthlyBudgetExpense(originalExpenseName, createdAtUtc);

        // Act
        expense.UpdateExpenseName(newExpenseName, updatedAtUtc);

        // Assert
        expense.ExpenseName.Should().Be(newExpenseName);
        expense.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void UpdateExpenseName_Should_PublishMonthlyBudgetExpenseUpdatedDomainEvent()
    {
        // Arrange
        string originalExpenseName = "Original Expense";
        string newExpenseName = "Updated Expense";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        MonthlyBudgetExpense expense = CreateMonthlyBudgetExpense(originalExpenseName, createdAtUtc);

        // Act
        expense.UpdateExpenseName(newExpenseName, updatedAtUtc);

        // Assert
        MonthlyBudgetExpenseUpdatedDomainEvent domainEvent = 
            AssertDomainEventWasPublished<MonthlyBudgetExpenseUpdatedDomainEvent>(expense);
        domainEvent.MonthlyBudgetExpenseId.Should().Be(expense.Id);
        domainEvent.MonthlyBudgetId.Should().Be(expense.MonthlyBudgetId);
        domainEvent.ExpenseId.Should().Be(expense.ExpenseId);
        domainEvent.ExpenseName.Should().Be(newExpenseName);
        domainEvent.OccurredOnUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void UpdateExpenseName_Should_NotUpdateOrPublishEvent_WhenNameIsTheSame()
    {
        // Arrange
        string expenseName = "Same Expense Name";
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        MonthlyBudgetExpense expense = CreateMonthlyBudgetExpense(expenseName, createdAtUtc);
        DateTime? originalUpdatedAt = expense.UpdatedAtUtc;

        // Act
        expense.UpdateExpenseName(expenseName, updatedAtUtc); // Same name

        // Assert
        expense.ExpenseName.Should().Be(expenseName);
        expense.UpdatedAtUtc.Should().Be(originalUpdatedAt); // Should not be updated

        // Should only have the creation event, not the update event
        expense.DomainEvents.Should().HaveCount(1);
        expense.DomainEvents.Should().AllBeOfType<MonthlyBudgetExpenseCreatedDomainEvent>();
    }

    [Fact]
    public void SetValue_Should_SetExpenseValue()
    {
        // Arrange
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        decimal value = Faker.Random.Decimal(1, 1000);

        MonthlyBudgetExpense expense = CreateMonthlyBudgetExpense("Test Expense", createdAtUtc);

        // Act
        expense.SetValue(value, updatedAtUtc);

        // Assert
        expense.Value.Should().Be(value);
        expense.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void SetValue_Should_PublishMonthlyBudgetExpenseValueSetDomainEvent()
    {
        // Arrange
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        decimal value = Faker.Random.Decimal(1, 1000);

        MonthlyBudgetExpense expense = CreateMonthlyBudgetExpense("Test Expense", createdAtUtc);

        // Act
        expense.SetValue(value, updatedAtUtc);

        // Assert
        MonthlyBudgetExpenseValueSetDomainEvent domainEvent = 
            AssertDomainEventWasPublished<MonthlyBudgetExpenseValueSetDomainEvent>(expense);
        domainEvent.MonthlyBudgetExpenseId.Should().Be(expense.Id);
        domainEvent.MonthlyBudgetId.Should().Be(expense.MonthlyBudgetId);
        domainEvent.Value.Should().Be(value);
        domainEvent.OccurredOnUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void SetValue_Should_HandleNullValue()
    {
        // Arrange
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        MonthlyBudgetExpense expense = CreateMonthlyBudgetExpense("Test Expense", createdAtUtc);
        
        // Set initial value
        expense.SetValue(100.50m, createdAtUtc);

        // Act
        expense.SetValue(null, updatedAtUtc); // Clear the value

        // Assert
        expense.Value.Should().BeNull();
        expense.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void SetValue_Should_NotUpdateOrPublishEvent_WhenValueIsTheSame()
    {
        // Arrange
        decimal value = 150.75m;
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime firstUpdatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        DateTime secondUpdatedAtUtc = firstUpdatedAtUtc.AddMinutes(10);

        MonthlyBudgetExpense expense = CreateMonthlyBudgetExpense("Test Expense", createdAtUtc);
        expense.SetValue(value, firstUpdatedAtUtc);
        DateTime? previousUpdatedAt = expense.UpdatedAtUtc;

        // Act
        expense.SetValue(value, secondUpdatedAtUtc); // Same value

        // Assert
        expense.Value.Should().Be(value);
        expense.UpdatedAtUtc.Should().Be(previousUpdatedAt); // Should not be updated

        // Should have creation event + one value set event, not two
        expense.DomainEvents.Should().HaveCount(2);
        expense.DomainEvents.OfType<MonthlyBudgetExpenseValueSetDomainEvent>().Should().HaveCount(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50.25)]
    [InlineData(999.99)]
    [InlineData(1500.00)]
    public void SetValue_Should_HandleVariousDecimalValues(decimal value)
    {
        // Arrange
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        MonthlyBudgetExpense expense = CreateMonthlyBudgetExpense("Test Expense", createdAtUtc);

        // Act
        expense.SetValue(value, updatedAtUtc);

        // Assert
        expense.Value.Should().Be(value);
        expense.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void SetValue_Should_HandleMultipleValueChanges()
    {
        // Arrange
        DateTime createdAtUtc = Faker.Date.PastOffset().UtcDateTime;
        DateTime firstUpdatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;
        DateTime secondUpdatedAtUtc = firstUpdatedAtUtc.AddMinutes(10);

        MonthlyBudgetExpense expense = CreateMonthlyBudgetExpense("Test Expense", createdAtUtc);

        // Act
        expense.SetValue(100.50m, firstUpdatedAtUtc);
        expense.SetValue(200.75m, secondUpdatedAtUtc);

        // Assert
        expense.Value.Should().Be(200.75m);
        expense.UpdatedAtUtc.Should().Be(secondUpdatedAtUtc);
        
        // Should have 3 events: Create + 2 Value Sets
        expense.DomainEvents.Should().HaveCount(3);
        expense.DomainEvents.Should().ContainSingle(e => e is MonthlyBudgetExpenseCreatedDomainEvent);
        expense.DomainEvents.OfType<MonthlyBudgetExpenseValueSetDomainEvent>().Should().HaveCount(2);
    }

    private MonthlyBudgetExpense CreateMonthlyBudgetExpense(string expenseName, DateTime createdAtUtc)
    {
        return MonthlyBudgetExpense.Create(
            "mb_" + Faker.Random.Guid(),
            "e_" + Faker.Random.Guid(),
            expenseName,
            "et_" + Faker.Random.Guid(),
            Faker.Commerce.Categories(1)[0],
            createdAtUtc);
    }
}
