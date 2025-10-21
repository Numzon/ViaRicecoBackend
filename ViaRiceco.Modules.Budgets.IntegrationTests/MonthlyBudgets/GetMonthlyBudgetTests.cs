using System.Net;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ViaRiceco.Modules.Budgets.Domain.MonthlyBudgets;
using ViaRiceco.Modules.Budgets.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.IntegrationTests.MonthlyBudgets;

public sealed class GetMonthlyBudgetTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnMonthlyBudget_WhenMonthlyBudgetExists()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        MonthlyBudget monthlyBudget = await CreateMonthlyBudgetWithExpensesAsync();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/monthly-budgets/{monthlyBudget.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        string content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(monthlyBudget.Id);
        content.Should().Contain("\"Month\":");
        content.Should().Contain("\"Year\":");
        content.Should().Contain("\"NetValue\":");
        content.Should().Contain("\"Expenses\":");
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenMonthlyBudgetDoesNotExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        string nonExistentMonthlyBudgetId = "mb_" + Guid.NewGuid();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/monthly-budgets/{nonExistentMonthlyBudgetId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnMonthlyBudgetWithHateoasLinks_WhenAcceptHeaderIncludesHateoas()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.via-riceco.hateoas.1+json"));
        
        MonthlyBudget monthlyBudget = await CreateMonthlyBudgetWithExpensesAsync();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/monthly-budgets/{monthlyBudget.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        string content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("_links");
        content.Should().Contain("self");
        content.Should().Contain("finalize");
        content.Should().Contain("set-as-draft");
        content.Should().Contain("bulk-set-values");
    }

    [Fact]
    public async Task Should_ReturnMonthlyBudgetWithoutHateoasLinks_WhenAcceptHeaderIsStandardJson()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        MonthlyBudget monthlyBudget = await CreateMonthlyBudgetWithExpensesAsync();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/monthly-budgets/{monthlyBudget.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        string content = await response.Content.ReadAsStringAsync();
        content.Should().NotContain("_links");
        content.Should().Contain(monthlyBudget.Id);
    }

    [Fact]
    public async Task Should_ReturnMonthlyBudgetWithExpenses()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        MonthlyBudget monthlyBudget = await CreateMonthlyBudgetWithExpensesAsync();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/monthly-budgets/{monthlyBudget.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        string content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"Expenses\":");
        content.Should().Contain("\"expenseName\":");
        content.Should().Contain("\"expenseTypeName\":");
        content.Should().Contain("\"value\":");
        
        // Should have 3 expenses from CreateMonthlyBudgetWithExpensesAsync
        MatchCollection expenseMatches = Regex.Matches(content, "\"expenseName\":");
        expenseMatches.Count.Should().Be(3);
    }

    [Fact]
    public async Task Should_SupportFieldSelection()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        MonthlyBudget monthlyBudget = await CreateMonthlyBudgetWithExpensesAsync();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/monthly-budgets/{monthlyBudget.Id}?fields=month,year,netValue");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        string content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"Month\":");
        content.Should().Contain("\"Year\":");
        content.Should().Contain("\"NetValue\":");
        content.Should().NotContain("\"expenses\":");
        content.Should().NotContain("\"CreatedAtUtc\":");
        content.Should().NotContain("\"updatedAtUtc\":");
    }

    [Fact]
    public async Task Should_ReturnMonthlyBudgetWithCorrectContentType()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        MonthlyBudget monthlyBudget = await CreateMonthlyBudgetWithExpensesAsync();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/monthly-budgets/{monthlyBudget.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    private async Task<MonthlyBudget> CreateMonthlyBudgetWithExpensesAsync()
    {
        // Create expense types with unique names
        var expenseType1 = Domain.ExpenseTypes.ExpenseType.Create($"Food-{Guid.NewGuid()}", DateTime.UtcNow);
        var expenseType2 = Domain.ExpenseTypes.ExpenseType.Create($"Transportation-{Guid.NewGuid()}", DateTime.UtcNow);
        var expenseType3 = Domain.ExpenseTypes.ExpenseType.Create($"Housing-{Guid.NewGuid()}", DateTime.UtcNow);

        DbContext.ExpenseTypes.AddRange(expenseType1, expenseType2, expenseType3);
        await DbContext.SaveChangesAsync();

        // Create expenses
        var expense1 = Domain.Expenses.Expense.Create("Groceries", expenseType1.Id, null, DateTime.UtcNow);
        var expense2 = Domain.Expenses.Expense.Create("Gas", expenseType2.Id, null, DateTime.UtcNow);
        var expense3 = Domain.Expenses.Expense.Create("Rent", expenseType3.Id, null, DateTime.UtcNow);

        DbContext.Expenses.AddRange(expense1, expense2, expense3);
        await DbContext.SaveChangesAsync();

        var expenseData = new List<Domain.MonthlyBudgets.ExpenseData>
        {
            new Domain.MonthlyBudgets.ExpenseData(expense1, expenseType1),
            new Domain.MonthlyBudgets.ExpenseData(expense2, expenseType2),
            new Domain.MonthlyBudgets.ExpenseData(expense3, expenseType3)
        };

        // Create monthly budget
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
