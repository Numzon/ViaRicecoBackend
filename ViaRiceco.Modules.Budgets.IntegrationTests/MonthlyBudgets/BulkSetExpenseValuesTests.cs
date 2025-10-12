using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgetExpenses;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;
using ViaRiceco.Modules.Budgets.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.IntegrationTests.MonthlyBudgets;

public sealed class BulkSetExpenseValuesTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private sealed class BulkSetExpenseValuesRequest
    {
        public string MonthlyBudgetId { get; init; } = string.Empty;
        public IReadOnlyCollection<ExpenseValueUpdateRequest> ExpenseValueUpdates { get; init; } = [];
    }

    private sealed class ExpenseValueUpdateRequest
    {
        public string MonthlyBudgetExpenseId { get; init; } = string.Empty;
        public decimal? Value { get; init; }
    }

    [Fact]
    public async Task Should_UpdateExpenseValues_WhenCommandIsValid()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        // Create a monthly budget with expenses for testing
        MonthlyBudget monthlyBudget = await CreateMonthlyBudgetWithExpensesAsync();
        
        MonthlyBudgetExpense expense1 = monthlyBudget.Expenses.First();
        MonthlyBudgetExpense expense2 = monthlyBudget.Expenses.Skip(1).First();
        
        var request = new BulkSetExpenseValuesRequest
        {
            MonthlyBudgetId = monthlyBudget.Id,
            ExpenseValueUpdates = new List<ExpenseValueUpdateRequest>
            {
                new() { MonthlyBudgetExpenseId = expense1.Id, Value = 150.75m },
                new() { MonthlyBudgetExpenseId = expense2.Id, Value = 300.50m }
            }
        };

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/budgets/monthly-budgets/{monthlyBudget.Id}/expenses/values", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Clear the context to ensure we get fresh data from the database
        DbContext.ChangeTracker.Clear();

        // Verify the values were updated in the database
        MonthlyBudget? updatedBudget = await DbContext.MonthlyBudgets
            .Include(mb => mb.Expenses)
            .FirstOrDefaultAsync(mb => mb.Id == monthlyBudget.Id);

        updatedBudget.Should().NotBeNull();
        
        MonthlyBudgetExpense updatedExpense1 = updatedBudget.Expenses.First(e => e.Id == expense1.Id);
        MonthlyBudgetExpense updatedExpense2 = updatedBudget.Expenses.First(e => e.Id == expense2.Id);
        
        updatedExpense1.Value.Should().Be(150.75m);
        updatedExpense2.Value.Should().Be(300.50m);
        updatedBudget.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenMonthlyBudgetDoesNotExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        string nonExistentBudgetId = "mb_" + Guid.NewGuid();
        var request = new BulkSetExpenseValuesRequest
        {
            MonthlyBudgetId = nonExistentBudgetId,
            ExpenseValueUpdates = new List<ExpenseValueUpdateRequest>
            {
                new() { MonthlyBudgetExpenseId = "mbe_" + Guid.NewGuid(), Value = 100.50m }
            }
        };

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/budgets/monthly-budgets/{nonExistentBudgetId}/expenses/values", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenExpenseNotFound()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        MonthlyBudget monthlyBudget = await CreateMonthlyBudgetWithExpensesAsync();
        
        string nonExistentExpenseId = "mbe_" + Guid.NewGuid();
        var request = new BulkSetExpenseValuesRequest
        {
            MonthlyBudgetId = monthlyBudget.Id,
            ExpenseValueUpdates = new List<ExpenseValueUpdateRequest>
            {
                new() { MonthlyBudgetExpenseId = nonExistentExpenseId, Value = 100.50m }
            }
        };

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/budgets/monthly-budgets/{monthlyBudget.Id}/expenses/values", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        MonthlyBudget monthlyBudget = await CreateMonthlyBudgetWithExpensesAsync();
        
        var request = new BulkSetExpenseValuesRequest
        {
            MonthlyBudgetId = monthlyBudget.Id,
            ExpenseValueUpdates = new List<ExpenseValueUpdateRequest>
            {
                new() { MonthlyBudgetExpenseId = string.Empty, Value = -100.50m } // Invalid: empty ID and negative value
            }
        };

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/budgets/monthly-budgets/{monthlyBudget.Id}/expenses/values", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_HandleNullValues_WhenUpdatingExpenses()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        MonthlyBudget monthlyBudget = await CreateMonthlyBudgetWithExpensesAsync();
        MonthlyBudgetExpense expense = monthlyBudget.Expenses.First();
        
        // Set initial value
        expense.SetValue(100.50m, DateTime.UtcNow);
        await DbContext.SaveChangesAsync();
        
        var request = new BulkSetExpenseValuesRequest
        {
            MonthlyBudgetId = monthlyBudget.Id,
            ExpenseValueUpdates = new List<ExpenseValueUpdateRequest>
            {
                new() { MonthlyBudgetExpenseId = expense.Id, Value = null } // Clear the value
            }
        };

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/budgets/monthly-budgets/{monthlyBudget.Id}/expenses/values", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Clear the context to ensure we get fresh data from the database
        DbContext.ChangeTracker.Clear();

        // Verify the value was cleared in the database
        MonthlyBudget? updatedBudget = await DbContext.MonthlyBudgets
            .Include(mb => mb.Expenses)
            .FirstOrDefaultAsync(mb => mb.Id == monthlyBudget.Id);

        MonthlyBudgetExpense updatedExpense = updatedBudget!.Expenses.First(e => e.Id == expense.Id);
        updatedExpense.Value.Should().BeNull();
    }

    [Fact]
    public async Task Should_UpdateOnlyChangedValues_WhenSomeValuesAreSame()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        MonthlyBudget monthlyBudget = await CreateMonthlyBudgetWithExpensesAsync();
        MonthlyBudgetExpense expense1 = monthlyBudget.Expenses.First();
        MonthlyBudgetExpense expense2 = monthlyBudget.Expenses.Skip(1).First();
        
        // Set initial values
        expense1.SetValue(100.50m, DateTime.UtcNow);
        expense2.SetValue(200.75m, DateTime.UtcNow);
        await DbContext.SaveChangesAsync();
        
        var request = new BulkSetExpenseValuesRequest
        {
            MonthlyBudgetId = monthlyBudget.Id,
            ExpenseValueUpdates = new List<ExpenseValueUpdateRequest>
            {
                new() { MonthlyBudgetExpenseId = expense1.Id, Value = 100.50m }, // Same value - no change
                new() { MonthlyBudgetExpenseId = expense2.Id, Value = 300.25m }  // Different value - should change
            }
        };

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/budgets/monthly-budgets/{monthlyBudget.Id}/expenses/values", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Clear the context to ensure we get fresh data from the database
        DbContext.ChangeTracker.Clear();

        // Verify the correct values in the database
        MonthlyBudget? updatedBudget = await DbContext.MonthlyBudgets
            .Include(mb => mb.Expenses)
            .FirstOrDefaultAsync(mb => mb.Id == monthlyBudget.Id);

        MonthlyBudgetExpense updatedExpense1 = updatedBudget!.Expenses.First(e => e.Id == expense1.Id);
        MonthlyBudgetExpense updatedExpense2 = updatedBudget.Expenses.First(e => e.Id == expense2.Id);
        
        updatedExpense1.Value.Should().Be(100.50m);
        updatedExpense2.Value.Should().Be(300.25m);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenTooManyExpenseUpdates()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        MonthlyBudget monthlyBudget = await CreateMonthlyBudgetWithExpensesAsync();
        
        // Create 101 updates (exceeds the limit of 100)
        var expenseUpdates = new List<ExpenseValueUpdateRequest>();
        for (int i = 0; i < 101; i++)
        {
            expenseUpdates.Add(new ExpenseValueUpdateRequest { MonthlyBudgetExpenseId = "mbe_" + Guid.NewGuid(), Value = 100m });
        }
        
        var request = new BulkSetExpenseValuesRequest
        {
            MonthlyBudgetId = monthlyBudget.Id,
            ExpenseValueUpdates = expenseUpdates
        };

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/budgets/monthly-budgets/{monthlyBudget.Id}/expenses/values", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_HandleEmptyUpdatesList()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        MonthlyBudget monthlyBudget = await CreateMonthlyBudgetWithExpensesAsync();
        
        var request = new BulkSetExpenseValuesRequest
        {
            MonthlyBudgetId = monthlyBudget.Id,
            ExpenseValueUpdates = new List<ExpenseValueUpdateRequest>()
        };

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/budgets/monthly-budgets/{monthlyBudget.Id}/expenses/values", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest); 
    }

    [Fact]
    public async Task Should_FailFast_WhenFirstExpenseNotFound()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        MonthlyBudget monthlyBudget = await CreateMonthlyBudgetWithExpensesAsync();
        MonthlyBudgetExpense validExpense = monthlyBudget.Expenses.First();
        
        // Set initial value for the valid expense
        validExpense.SetValue(100.50m, DateTime.UtcNow);
        await DbContext.SaveChangesAsync();
        
        string nonExistentExpenseId = "mbe_" + Guid.NewGuid();
        var request = new BulkSetExpenseValuesRequest
        {
            MonthlyBudgetId = monthlyBudget.Id,
            ExpenseValueUpdates = new List<ExpenseValueUpdateRequest>
            {
                new() { MonthlyBudgetExpenseId = nonExistentExpenseId, Value = 200.75m }, // This should fail
                new() { MonthlyBudgetExpenseId = validExpense.Id, Value = 300.25m }       // This should not be processed due to fail-fast
            }
        };

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/budgets/monthly-budgets/{monthlyBudget.Id}/expenses/values", 
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify the valid expense was not updated due to fail-fast behavior
        MonthlyBudget? unchangedBudget = await DbContext.MonthlyBudgets
            .Include(mb => mb.Expenses)
            .FirstOrDefaultAsync(mb => mb.Id == monthlyBudget.Id);

        MonthlyBudgetExpense unchangedExpense = unchangedBudget!.Expenses.First(e => e.Id == validExpense.Id);
        unchangedExpense.Value.Should().Be(100.50m); // Should remain unchanged
    }

    private async Task<MonthlyBudget> CreateMonthlyBudgetWithExpensesAsync()
    {
        // Create expense types first
        var expenseType1 = Domain.ExpenseTypes.ExpenseType.Create("Food", DateTime.UtcNow);
        var expenseType2 = Domain.ExpenseTypes.ExpenseType.Create("Transportation", DateTime.UtcNow);
        var expenseType3 = Domain.ExpenseTypes.ExpenseType.Create("Housing", DateTime.UtcNow);

        DbContext.ExpenseTypes.AddRange(expenseType1, expenseType2, expenseType3);
        await DbContext.SaveChangesAsync();

        // Create expenses
        var expense1 = Domain.Expenses.Expense.Create("Groceries", expenseType1.Id, null, DateTime.UtcNow);
        var expense2 = Domain.Expenses.Expense.Create("Gas", expenseType2.Id, null, DateTime.UtcNow);
        var expense3 = Domain.Expenses.Expense.Create("Rent", expenseType3.Id, null, DateTime.UtcNow);

        DbContext.Expenses.AddRange(expense1, expense2, expense3);
        await DbContext.SaveChangesAsync();
        
        var expenseData = new List<ExpenseData>
        {
            new ExpenseData(expense1, expenseType1),
            new ExpenseData(expense2, expenseType2),
            new ExpenseData(expense3, expenseType3)
        };
        
        var monthlyBudget = MonthlyBudget.CreateFromSettlementPeriod(
            "sp_" + Guid.NewGuid(),
            Faker.Random.Int(1, 12),
            Faker.Random.Int(2020, 2025),
            Faker.Random.Decimal(1000, 5000),
            expenseData,
            DateTime.UtcNow);

        DbContext.MonthlyBudgets.Add(monthlyBudget);
        await DbContext.SaveChangesAsync();

        return monthlyBudget;
    }
}
