using FluentAssertions;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgetExpenses;
using ViaRiceco.Modules.Budgets.Domain.Expenses;
using ViaRiceco.Modules.Budgets.Domain.ExpenseTypes;
using ViaRiceco.Modules.Budgets.UnitTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.UnitTests.MonthlyBudgets;

public sealed class MonthlyBudgetTests : BaseTest
{
    [Fact]
    public void BulkSetExpenseValues_Should_UpdateExpenseValues_WhenAllExpensesExist()
    {
        // Arrange
        MonthlyBudget monthlyBudget = CreateMonthlyBudgetWithExpenses();
        MonthlyBudgetExpense expense1 = monthlyBudget.Expenses.First();
        MonthlyBudgetExpense expense2 = monthlyBudget.Expenses.Skip(1).First();
        
        var updates = new List<ExpenseValueUpdate>
        {
            new(expense1.Id, 100.50m),
            new(expense2.Id, 200.75m)
        };
        
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result result = monthlyBudget.BulkSetExpenseValues(updates, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        expense1.Value.Should().Be(100.50m);
        expense2.Value.Should().Be(200.75m);
        monthlyBudget.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void BulkSetExpenseValues_Should_ReturnFailure_WhenExpenseNotFound()
    {
        // Arrange
        MonthlyBudget monthlyBudget = CreateMonthlyBudgetWithExpenses();
        string nonExistentExpenseId = "mbe_" + Faker.Random.Guid();
        
        var updates = new List<ExpenseValueUpdate>
        {
            new(nonExistentExpenseId, 100.50m)
        };
        
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result result = monthlyBudget.BulkSetExpenseValues(updates, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("MonthlyBudget.ExpenseNotFound");
    }

    [Fact]
    public void BulkSetExpenseValues_Should_FailFast_WhenFirstExpenseNotFound()
    {
        // Arrange
        MonthlyBudget monthlyBudget = CreateMonthlyBudgetWithExpenses();
        MonthlyBudgetExpense validExpense = monthlyBudget.Expenses.First();
        string nonExistentExpenseId = "mbe_" + Faker.Random.Guid();
        
        var updates = new List<ExpenseValueUpdate>
        {
            new(nonExistentExpenseId, 100.50m), // This should fail
            new(validExpense.Id, 200.75m)       // This should not be processed
        };
        
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result result = monthlyBudget.BulkSetExpenseValues(updates, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeFalse();
        validExpense.Value.Should().BeNull(); // Should not be updated due to fail-fast
        monthlyBudget.UpdatedAtUtc.Should().NotBe(updatedAtUtc); // Should not be updated
    }

    [Fact]
    public void BulkSetExpenseValues_Should_OnlyUpdateChangedValues()
    {
        // Arrange
        MonthlyBudget monthlyBudget = CreateMonthlyBudgetWithExpenses();
        MonthlyBudgetExpense expense1 = monthlyBudget.Expenses.First();
        MonthlyBudgetExpense expense2 = monthlyBudget.Expenses.Skip(1).First();
        
        // Set initial values
        expense1.SetValue(100.50m, Faker.Date.PastOffset().UtcDateTime);
        expense2.SetValue(200.75m, Faker.Date.PastOffset().UtcDateTime);
        
        var updates = new List<ExpenseValueUpdate>
        {
            new(expense1.Id, 100.50m), // Same value - no change
            new(expense2.Id, 300.25m)  // Different value - should change
        };
        
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result result = monthlyBudget.BulkSetExpenseValues(updates, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        expense1.Value.Should().Be(100.50m);
        expense2.Value.Should().Be(300.25m);
        monthlyBudget.UpdatedAtUtc.Should().Be(updatedAtUtc); // Should be updated because expense2 changed
    }

    [Fact]
    public void BulkSetExpenseValues_Should_NotUpdateTimestamp_WhenNoChanges()
    {
        // Arrange
        MonthlyBudget monthlyBudget = CreateMonthlyBudgetWithExpenses();
        MonthlyBudgetExpense expense = monthlyBudget.Expenses.First();
        
        // Set initial value
        expense.SetValue(100.50m, Faker.Date.PastOffset().UtcDateTime);
        DateTime? originalUpdatedAt = monthlyBudget.UpdatedAtUtc;
        
        var updates = new List<ExpenseValueUpdate>
        {
            new(expense.Id, 100.50m) // Same value - no change
        };
        
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result result = monthlyBudget.BulkSetExpenseValues(updates, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        monthlyBudget.UpdatedAtUtc.Should().Be(originalUpdatedAt); // Should not be updated
    }

    [Fact]
    public void BulkSetExpenseValues_Should_HandleNullValues()
    {
        // Arrange
        MonthlyBudget monthlyBudget = CreateMonthlyBudgetWithExpenses();
        MonthlyBudgetExpense expense = monthlyBudget.Expenses.First();
        
        // Set initial value
        expense.SetValue(100.50m, Faker.Date.PastOffset().UtcDateTime);
        
        var updates = new List<ExpenseValueUpdate>
        {
            new(expense.Id, null) // Clear the value
        };
        
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result result = monthlyBudget.BulkSetExpenseValues(updates, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        expense.Value.Should().BeNull();
        monthlyBudget.UpdatedAtUtc.Should().Be(updatedAtUtc);
    }

    [Fact]
    public void BulkSetExpenseValues_Should_HandleEmptyUpdatesList()
    {
        // Arrange
        MonthlyBudget monthlyBudget = CreateMonthlyBudgetWithExpenses();
        var updates = new List<ExpenseValueUpdate>();
        DateTime updatedAtUtc = Faker.Date.RecentOffset().UtcDateTime;

        // Act
        Result result = monthlyBudget.BulkSetExpenseValues(updates, updatedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        monthlyBudget.UpdatedAtUtc.Should().NotBe(updatedAtUtc); // Should not be updated
    }

    private MonthlyBudget CreateMonthlyBudgetWithExpenses()
    {
        var expense1 = Expense.Create("Groceries", "et_" + Faker.Random.Guid(), null, Faker.Date.PastOffset().UtcDateTime);
        var expense2 = Expense.Create("Gas", "et_" + Faker.Random.Guid(), null, Faker.Date.PastOffset().UtcDateTime);
        var expense3 = Expense.Create("Rent", "et_" + Faker.Random.Guid(), null, Faker.Date.PastOffset().UtcDateTime);

        var expenseType1 = ExpenseType.Create("Food", Faker.Date.PastOffset().UtcDateTime);
        var expenseType2 = ExpenseType.Create("Transportation", Faker.Date.PastOffset().UtcDateTime);
        var expenseType3 = ExpenseType.Create("Housing", Faker.Date.PastOffset().UtcDateTime);

        var expenseData = new List<ExpenseData>
        {
            new ExpenseData(expense1, expenseType1),
            new ExpenseData(expense2, expenseType2),
            new ExpenseData(expense3, expenseType3)
        };

        return MonthlyBudget.CreateFromSettlementPeriod(
            "sp_" + Faker.Random.Guid(),
            Faker.Random.Int(1, 12),
            Faker.Random.Int(2020, 2025),
            Faker.Random.Decimal(1000, 5000),
            expenseData,
            Faker.Date.RecentOffset().UtcDateTime);
    }
}
