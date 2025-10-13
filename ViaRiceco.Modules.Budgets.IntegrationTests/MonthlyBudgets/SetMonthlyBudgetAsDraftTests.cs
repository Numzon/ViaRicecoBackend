using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;
using ViaRiceco.Modules.Budgets.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.IntegrationTests.MonthlyBudgets;

public sealed class SetMonthlyBudgetAsDraftTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_SetMonthlyBudgetAsDraft_WhenMonthlyBudgetExistsAndIsFinalized()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        MonthlyBudget monthlyBudget = await CreateFinalizedMonthlyBudgetAsync();

        // Act
        using var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync($"/api/budgets/monthly-budgets/{monthlyBudget.Id}/draft", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the monthly budget was set as draft in the database
        DbContext.ChangeTracker.Clear();
        MonthlyBudget? draftBudget = await DbContext.MonthlyBudgets
            .FirstOrDefaultAsync(mb => mb.Id == monthlyBudget.Id);
        
        draftBudget.Should().NotBeNull();
        draftBudget!.IsDraft.Should().BeTrue();
        draftBudget.UpdatedAtUtc.Should().NotBeNull();
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
        HttpResponseMessage response = await client.PostAsync($"/api/budgets/monthly-budgets/{nonExistentMonthlyBudgetId}/draft", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnConflict_WhenMonthlyBudgetIsAlreadyDraft()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        MonthlyBudget monthlyBudget = await CreateDraftMonthlyBudgetAsync();

        // Act
        using var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync($"/api/budgets/monthly-budgets/{monthlyBudget.Id}/draft", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        
        // Verify the monthly budget remains draft
        DbContext.ChangeTracker.Clear();
        MonthlyBudget? unchangedBudget = await DbContext.MonthlyBudgets
            .FirstOrDefaultAsync(mb => mb.Id == monthlyBudget.Id);
        
        unchangedBudget.Should().NotBeNull();
        unchangedBudget!.IsDraft.Should().BeTrue();
    }

    [Fact]
    public async Task Should_ReturnConflict_WhenTryingToSetOlderMonthlyBudgetAsDraft()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        // Create two monthly budgets - one older, one newer
        MonthlyBudget olderBudget = await CreateFinalizedMonthlyBudgetWithSpecificDateAsync(10, 2024); // October 2024
        await CreateFinalizedMonthlyBudgetWithSpecificDateAsync(11, 2024); // November 2024

        // Act - Try to set the older budget as draft when newer one exists
        using var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync($"/api/budgets/monthly-budgets/{olderBudget.Id}/draft", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        
        // Verify the older budget remains finalized
        DbContext.ChangeTracker.Clear();
        MonthlyBudget? unchangedBudget = await DbContext.MonthlyBudgets
            .FirstOrDefaultAsync(mb => mb.Id == olderBudget.Id);
        
        unchangedBudget.Should().NotBeNull();
        unchangedBudget!.IsDraft.Should().BeFalse();
    }

    [Fact]
    public async Task Should_SetMostRecentMonthlyBudgetAsDraft_WhenItIsTheMostRecent()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        // Create two monthly budgets - one older, one newer
        MonthlyBudget olderBudget = await CreateFinalizedMonthlyBudgetWithSpecificDateAsync(10, 2024); // October 2024
        MonthlyBudget newerBudget = await CreateFinalizedMonthlyBudgetWithSpecificDateAsync(11, 2024); // November 2024

        // Act - Set the newer (most recent) budget as draft
        using var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync($"/api/budgets/monthly-budgets/{newerBudget.Id}/draft", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the newer budget was set as draft
        DbContext.ChangeTracker.Clear();
        MonthlyBudget? draftBudget = await DbContext.MonthlyBudgets
            .FirstOrDefaultAsync(mb => mb.Id == newerBudget.Id);
        
        draftBudget.Should().NotBeNull();
        draftBudget!.IsDraft.Should().BeTrue();
        
        // Verify the older budget remains finalized
        MonthlyBudget? finalizedBudget = await DbContext.MonthlyBudgets
            .FirstOrDefaultAsync(mb => mb.Id == olderBudget.Id);
        
        finalizedBudget.Should().NotBeNull();
        finalizedBudget!.IsDraft.Should().BeFalse();
    }

    [Fact]
    public async Task Should_UpdateTimestamp_WhenSettingMonthlyBudgetAsDraft()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        MonthlyBudget monthlyBudget = await CreateFinalizedMonthlyBudgetAsync();
        DateTime? originalUpdatedAt = monthlyBudget.UpdatedAtUtc;

        // Act
        using var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync($"/api/budgets/monthly-budgets/{monthlyBudget.Id}/draft", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the timestamp was updated
        DbContext.ChangeTracker.Clear();
        MonthlyBudget? draftBudget = await DbContext.MonthlyBudgets
            .FirstOrDefaultAsync(mb => mb.Id == monthlyBudget.Id);
        
        draftBudget.Should().NotBeNull();
        draftBudget!.IsDraft.Should().BeTrue();
        draftBudget.UpdatedAtUtc.Should().NotBe(originalUpdatedAt);
        draftBudget.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Should_HandleCrossYearComparison_WhenCheckingMostRecent()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        // Create budgets from different years
        MonthlyBudget olderBudget = await CreateFinalizedMonthlyBudgetWithSpecificDateAsync(12, 2023); // December 2023
        await CreateFinalizedMonthlyBudgetWithSpecificDateAsync(1, 2024);  // January 2024

        // Act - Try to set the older budget as draft (should fail)
        using var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync($"/api/budgets/monthly-budgets/{olderBudget.Id}/draft", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        
        // Verify the older budget remains finalized
        DbContext.ChangeTracker.Clear();
        MonthlyBudget? unchangedBudget = await DbContext.MonthlyBudgets
            .FirstOrDefaultAsync(mb => mb.Id == olderBudget.Id);
        
        unchangedBudget.Should().NotBeNull();
        unchangedBudget!.IsDraft.Should().BeFalse();
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

    private async Task<MonthlyBudget> CreateFinalizedMonthlyBudgetWithSpecificDateAsync(int month, int year)
    {
        // Create expense types
        var expenseType1 = Domain.ExpenseTypes.ExpenseType.Create($"Food {month}/{year}", DateTime.UtcNow);
        var expenseType2 = Domain.ExpenseTypes.ExpenseType.Create($"Transportation {month}/{year}", DateTime.UtcNow);

        DbContext.ExpenseTypes.AddRange(expenseType1, expenseType2);
        await DbContext.SaveChangesAsync();

        // Create expenses
        var expense1 = Domain.Expenses.Expense.Create($"Groceries {month}/{year}", expenseType1.Id, null, DateTime.UtcNow);
        var expense2 = Domain.Expenses.Expense.Create($"Gas {month}/{year}", expenseType2.Id, null, DateTime.UtcNow);

        DbContext.Expenses.AddRange(expense1, expense2);
        await DbContext.SaveChangesAsync();

        var expenseData = new List<Domain.MonthlyBudgets.ExpenseData>
        {
            new Domain.MonthlyBudgets.ExpenseData(expense1, expenseType1),
            new Domain.MonthlyBudgets.ExpenseData(expense2, expenseType2)
        };

        // Create monthly budget with specific month and year
        var monthlyBudget = MonthlyBudget.CreateFromSettlementPeriod(
            "sp_" + Guid.NewGuid(),
            month,
            year,
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

        // Finalize the budget
        monthlyBudget.Finalize(DateTime.UtcNow);
        
        await DbContext.SaveChangesAsync();

        return monthlyBudget;
    }
}
