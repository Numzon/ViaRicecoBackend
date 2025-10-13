using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;
using ViaRiceco.Modules.Budgets.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.IntegrationTests.MonthlyBudgets;

public sealed class FinalizeMonthlyBudgetTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_FinalizeMonthlyBudget_WhenMonthlyBudgetExistsAndIsDraft()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        MonthlyBudget monthlyBudget = await CreateDraftMonthlyBudgetAsync();

        // Act
        using var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync($"/api/budgets/monthly-budgets/{monthlyBudget.Id}/finalize", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the monthly budget was finalized in the database
        DbContext.ChangeTracker.Clear();
        MonthlyBudget? finalizedBudget = await DbContext.MonthlyBudgets
            .FirstOrDefaultAsync(mb => mb.Id == monthlyBudget.Id);
        
        finalizedBudget.Should().NotBeNull();
        finalizedBudget!.IsDraft.Should().BeFalse();
        finalizedBudget.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenMonthlyBudgetDoesNotExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        string nonExistentMonthlyBudgetId = "mb_" + Guid.NewGuid();

        // Act
        using var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync($"/api/budgets/monthly-budgets/{nonExistentMonthlyBudgetId}/finalize", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnConflict_WhenMonthlyBudgetIsAlreadyFinalized()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        MonthlyBudget monthlyBudget = await CreateFinalizedMonthlyBudgetAsync();

        // Act
        using var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync($"/api/budgets/monthly-budgets/{monthlyBudget.Id}/finalize", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        
        // Verify the monthly budget remains finalized
        DbContext.ChangeTracker.Clear();
        MonthlyBudget? unchangedBudget = await DbContext.MonthlyBudgets
            .FirstOrDefaultAsync(mb => mb.Id == monthlyBudget.Id);
        
        unchangedBudget.Should().NotBeNull();
        unchangedBudget!.IsDraft.Should().BeFalse();
    }

    [Fact]
    public async Task Should_BeIdempotent_WhenFinalizingAlreadyFinalizedBudget()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        MonthlyBudget monthlyBudget = await CreateDraftMonthlyBudgetAsync();

        // Act - Finalize twice
        using var firstContent = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        using var secondContent = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage firstResponse = await client.PostAsync($"/api/budgets/monthly-budgets/{monthlyBudget.Id}/finalize", firstContent);
        HttpResponseMessage secondResponse = await client.PostAsync($"/api/budgets/monthly-budgets/{monthlyBudget.Id}/finalize", secondContent);

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict); // Already finalized
        
        // Verify the monthly budget is finalized
        DbContext.ChangeTracker.Clear();
        MonthlyBudget? finalizedBudget = await DbContext.MonthlyBudgets
            .FirstOrDefaultAsync(mb => mb.Id == monthlyBudget.Id);
        
        finalizedBudget.Should().NotBeNull();
        finalizedBudget!.IsDraft.Should().BeFalse();
    }

    [Fact]
    public async Task Should_UpdateTimestamp_WhenFinalizingMonthlyBudget()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        MonthlyBudget monthlyBudget = await CreateDraftMonthlyBudgetAsync();
        DateTime? originalUpdatedAt = monthlyBudget.UpdatedAtUtc;

        // Act
        using var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync($"/api/budgets/monthly-budgets/{monthlyBudget.Id}/finalize", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the timestamp was updated
        DbContext.ChangeTracker.Clear();
        MonthlyBudget? finalizedBudget = await DbContext.MonthlyBudgets
            .FirstOrDefaultAsync(mb => mb.Id == monthlyBudget.Id);
        
        finalizedBudget.Should().NotBeNull();
        finalizedBudget!.IsDraft.Should().BeFalse();
        finalizedBudget.UpdatedAtUtc.Should().NotBe(originalUpdatedAt);
        finalizedBudget.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_FinalizeMultipleMonthlyBudgets_WhenCalledForDifferentBudgets()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        MonthlyBudget budget1 = await CreateDraftMonthlyBudgetAsync();
        MonthlyBudget budget2 = await CreateDraftMonthlyBudgetAsync();
        MonthlyBudget budget3 = await CreateDraftMonthlyBudgetAsync();

        // Act
        using var content1 = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        using var content2 = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        using var content3 = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response1 = await client.PostAsync($"/api/budgets/monthly-budgets/{budget1.Id}/finalize", content1);
        HttpResponseMessage response2 = await client.PostAsync($"/api/budgets/monthly-budgets/{budget2.Id}/finalize", content2);
        HttpResponseMessage response3 = await client.PostAsync($"/api/budgets/monthly-budgets/{budget3.Id}/finalize", content3);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response2.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response3.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify all budgets were finalized
        DbContext.ChangeTracker.Clear();
        int finalizedBudgetsCount = await DbContext.MonthlyBudgets
            .CountAsync(mb => !mb.IsDraft);
        
        finalizedBudgetsCount.Should().Be(3);
    }

    private async Task<MonthlyBudget> CreateDraftMonthlyBudgetAsync()
    {
        // Create expense types with unique names
        var expenseType1 = Domain.ExpenseTypes.ExpenseType.Create($"Food-{Guid.NewGuid()}", DateTime.UtcNow);
        var expenseType2 = Domain.ExpenseTypes.ExpenseType.Create($"Transportation-{Guid.NewGuid()}", DateTime.UtcNow);

        DbContext.ExpenseTypes.AddRange(expenseType1, expenseType2);
        await DbContext.SaveChangesAsync();

        // Create expenses
        var expense1 = Domain.Expenses.Expense.Create("Groceries", expenseType1.Id, null, DateTime.UtcNow);
        var expense2 = Domain.Expenses.Expense.Create("Gas", expenseType2.Id, null, DateTime.UtcNow);

        DbContext.Expenses.AddRange(expense1, expense2);
        await DbContext.SaveChangesAsync();

        var expenseData = new List<Domain.MonthlyBudgets.ExpenseData>
        {
            new Domain.MonthlyBudgets.ExpenseData(expense1, expenseType1),
            new Domain.MonthlyBudgets.ExpenseData(expense2, expenseType2)
        };

        // Create monthly budget (it's draft by default)
        var monthlyBudget = MonthlyBudget.CreateFromSettlementPeriod(
            "sp_" + Guid.NewGuid(),
            Faker.Random.Int(1, 12),
            Faker.Random.Int(2020, 2025),
            Faker.Random.Decimal(1000, 5000),
            expenseData,
            DateTime.UtcNow);

        DbContext.MonthlyBudgets.Add(monthlyBudget);
        await DbContext.SaveChangesAsync();

        // Set values for all expenses so the budget can be finalized
        var expenseValueUpdates = new List<Domain.MonthlyBudgets.ExpenseValueUpdate>
        {
            new Domain.MonthlyBudgets.ExpenseValueUpdate(monthlyBudget.Expenses.First().Id, Faker.Random.Decimal(100, 500)),
            new Domain.MonthlyBudgets.ExpenseValueUpdate(monthlyBudget.Expenses.Last().Id, Faker.Random.Decimal(50, 200))
        };
        
        monthlyBudget.BulkSetExpenseValues(expenseValueUpdates, DateTime.UtcNow);
        await DbContext.SaveChangesAsync();

        return monthlyBudget;
    }

    private async Task<MonthlyBudget> CreateFinalizedMonthlyBudgetAsync()
    {
        MonthlyBudget monthlyBudget = await CreateDraftMonthlyBudgetAsync();
        
        // Finalize the budget
        monthlyBudget.Finalize(DateTime.UtcNow);
        await DbContext.SaveChangesAsync();
        
        return monthlyBudget;
    }
}
