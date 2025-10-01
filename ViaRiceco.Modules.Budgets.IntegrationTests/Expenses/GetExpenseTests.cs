using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.CreateExpenseType;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models;
using ViaRiceco.Modules.Budgets.Application.Expenses.CreateExpense;
using ViaRiceco.Modules.Budgets.Application.Expenses.Models;
using ViaRiceco.Modules.Budgets.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.IntegrationTests.Expenses;

public sealed class GetExpenseTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnExpense_WhenExpenseExists()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type and expense
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        string expenseName = "Hotel Booking";
        HttpResponseMessage createExpenseResponse = await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand(expenseName, expenseType!.Id, null));
        ExpenseDto? createdExpense = await createExpenseResponse.Content.ReadFromJsonAsync<ExpenseDto>();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/expenses/{createdExpense!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ExpenseDto? result = await response.Content.ReadFromJsonAsync<ExpenseDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdExpense.Id);
        result.Name.Should().Be(expenseName);
        result.ExpenseTypeId.Should().Be(expenseType.Id);
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenExpenseDoesNotExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        string nonExistentId = "e_" + Faker.Random.Guid();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/expenses/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnExpenseWithCorrectFormat()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type and expense
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        string expenseName = "Business Conference & Networking";
        HttpResponseMessage createExpenseResponse = await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand(expenseName, expenseType!.Id, null));
        ExpenseDto? createdExpense = await createExpenseResponse.Content.ReadFromJsonAsync<ExpenseDto>();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/expenses/{createdExpense!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ExpenseDto? result = await response.Content.ReadFromJsonAsync<ExpenseDto>();
        result.Should().NotBeNull();
        result!.Id.Should().StartWith("e_");
        result.Id.Should().HaveLength(38); // "e_" + GUID length
        result.Name.Should().Be(expenseName);
        result.ExpenseTypeId.Should().StartWith("et_");
    }

    [Fact]
    public async Task Should_ReturnDifferentExpenses_WhenRequestingDifferentIds()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        // Create multiple expenses
        HttpResponseMessage firstCreateResponse = await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Hotel Booking", expenseType!.Id, null));
        HttpResponseMessage secondCreateResponse = await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Flight Tickets", expenseType.Id, null));
        
        ExpenseDto? firstExpense = await firstCreateResponse.Content.ReadFromJsonAsync<ExpenseDto>();
        ExpenseDto? secondExpense = await secondCreateResponse.Content.ReadFromJsonAsync<ExpenseDto>();

        // Act
        HttpResponseMessage firstResponse = await client.GetAsync($"/api/budgets/expenses/{firstExpense!.Id}");
        HttpResponseMessage secondResponse = await client.GetAsync($"/api/budgets/expenses/{secondExpense!.Id}");

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ExpenseDto? firstResult = await firstResponse.Content.ReadFromJsonAsync<ExpenseDto>();
        ExpenseDto? secondResult = await secondResponse.Content.ReadFromJsonAsync<ExpenseDto>();

        firstResult!.Name.Should().Be("Hotel Booking");
        secondResult!.Name.Should().Be("Flight Tickets");
        firstResult.Id.Should().NotBe(secondResult.Id);
    }

    [Theory]
    [InlineData("e_invalid-format")]
    [InlineData("invalid-id")]
    public async Task Should_ReturnNotFound_WhenIdFormatIsInvalid(string invalidId)
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/expenses/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnExpenseWithUnicodeCharacters()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        string unicodeExpenseName = "Hôtel à Paris (パリのホテル) - فندق في باريس";
        HttpResponseMessage createExpenseResponse = await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand(unicodeExpenseName, expenseType!.Id, null));
        ExpenseDto? createdExpense = await createExpenseResponse.Content.ReadFromJsonAsync<ExpenseDto>();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/expenses/{createdExpense!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ExpenseDto? result = await response.Content.ReadFromJsonAsync<ExpenseDto>();
        result!.Name.Should().Be(unicodeExpenseName);
    }
}
