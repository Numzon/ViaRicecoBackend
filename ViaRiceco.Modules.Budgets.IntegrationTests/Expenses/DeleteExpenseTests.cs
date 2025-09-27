using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.CreateExpenseType;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models;
using ViaRiceco.Modules.Budgets.Application.Expenses.CreateExpense;
using ViaRiceco.Modules.Budgets.Application.Expenses.Models;
using ViaRiceco.Modules.Budgets.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.IntegrationTests.Expenses;

public sealed class DeleteExpenseTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_DeleteExpense_WhenExpenseExists()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type and expense
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        HttpResponseMessage createExpenseResponse = await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Hotel Booking", expenseType!.Id));
        ExpenseDto? createdExpense = await createExpenseResponse.Content.ReadFromJsonAsync<ExpenseDto>();

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/budgets/expenses/{createdExpense!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify expense is deleted
        HttpResponseMessage getResponse = await client.GetAsync($"/api/budgets/expenses/{createdExpense.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenExpenseDoesNotExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        string nonExistentId = "e_" + Faker.Random.Guid();

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/budgets/expenses/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_DeleteMultipleExpenses_Independently()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        // Create multiple expenses
        HttpResponseMessage firstCreateResponse = await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Hotel Booking", expenseType!.Id));
        HttpResponseMessage secondCreateResponse = await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Flight Tickets", expenseType.Id));
        
        ExpenseDto? firstExpense = await firstCreateResponse.Content.ReadFromJsonAsync<ExpenseDto>();
        ExpenseDto? secondExpense = await secondCreateResponse.Content.ReadFromJsonAsync<ExpenseDto>();

        // Act - Delete first expense
        HttpResponseMessage firstDeleteResponse = await client.DeleteAsync($"/api/budgets/expenses/{firstExpense!.Id}");

        // Assert
        firstDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify first is deleted but second still exists
        HttpResponseMessage firstGetResponse = await client.GetAsync($"/api/budgets/expenses/{firstExpense.Id}");
        HttpResponseMessage secondGetResponse = await client.GetAsync($"/api/budgets/expenses/{secondExpense!.Id}");

        firstGetResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        secondGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_DeleteExpenseWithoutAffectingOtherExpensesInSameExpenseType()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        // Create multiple expenses in same expense type
        string[] createExpenses =
        [
            "Hotel Booking",
            "Flight Tickets", 
            "Taxi Fare"
        ];

        var createdExpenses = new List<ExpenseDto>();
        foreach (string expenseName in createExpenses)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand(expenseName, expenseType!.Id));
            ExpenseDto? expense = await response.Content.ReadFromJsonAsync<ExpenseDto>();
            createdExpenses.Add(expense!);
        }

        // Act - Delete middle expense
        HttpResponseMessage deleteResponse = await client.DeleteAsync($"/api/budgets/expenses/{createdExpenses[1].Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify other expenses still exist
        HttpResponseMessage firstGetResponse = await client.GetAsync($"/api/budgets/expenses/{createdExpenses[0].Id}");
        HttpResponseMessage deletedGetResponse = await client.GetAsync($"/api/budgets/expenses/{createdExpenses[1].Id}");
        HttpResponseMessage thirdGetResponse = await client.GetAsync($"/api/budgets/expenses/{createdExpenses[2].Id}");

        firstGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        deletedGetResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        thirdGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_AllowDeletingExpenseFromDifferentExpenseTypes()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create two expense types
        HttpResponseMessage createTravelExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        HttpResponseMessage createOfficeExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Office"));
        
        ExpenseTypeDto? travelExpenseType = await createTravelExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();
        ExpenseTypeDto? officeExpenseType = await createOfficeExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        // Create expenses in different expense types
        HttpResponseMessage travelExpenseResponse = await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Hotel Booking", travelExpenseType!.Id));
        HttpResponseMessage officeExpenseResponse = await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Office Supplies", officeExpenseType!.Id));
        
        ExpenseDto? travelExpense = await travelExpenseResponse.Content.ReadFromJsonAsync<ExpenseDto>();
        ExpenseDto? officeExpense = await officeExpenseResponse.Content.ReadFromJsonAsync<ExpenseDto>();

        // Act - Delete travel expense
        HttpResponseMessage deleteResponse = await client.DeleteAsync($"/api/budgets/expenses/{travelExpense!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify travel expense is deleted but office expense remains
        HttpResponseMessage travelGetResponse = await client.GetAsync($"/api/budgets/expenses/{travelExpense.Id}");
        HttpResponseMessage officeGetResponse = await client.GetAsync($"/api/budgets/expenses/{officeExpense!.Id}");

        travelGetResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        officeGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
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
        HttpResponseMessage response = await client.DeleteAsync($"/api/budgets/expenses/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
