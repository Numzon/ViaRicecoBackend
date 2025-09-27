using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.CreateExpenseType;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models;
using ViaRiceco.Modules.Budgets.Application.Expenses.CreateExpense;
using ViaRiceco.Modules.Budgets.Application.Expenses.Models;
using ViaRiceco.Modules.Budgets.Application.Expenses.UpdateExpense;
using ViaRiceco.Modules.Budgets.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.IntegrationTests.Expenses;

public sealed class UpdateExpenseTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_UpdateExpense_WhenCommandIsValid()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        // Create expense
        HttpResponseMessage createExpenseResponse = await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Hotel Booking", expenseType!.Id));
        ExpenseDto? createdExpense = await createExpenseResponse.Content.ReadFromJsonAsync<ExpenseDto>();

        string updatedName = "Updated Hotel Booking";
        var updateRequest = new UpdateExpenseCommand(createdExpense!.Id, updatedName, expenseType.Id);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/expenses/{createdExpense.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ExpenseDto? result = await response.Content.ReadFromJsonAsync<ExpenseDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdExpense.Id);
        result.Name.Should().Be(updatedName);
        result.ExpenseTypeId.Should().Be(expenseType.Id);
    }

    [Fact]
    public async Task Should_UpdateExpenseToNewExpenseType_WhenExpenseTypeIsChanged()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create two expense types
        HttpResponseMessage createTravelExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        HttpResponseMessage createOfficeExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Office"));
        
        ExpenseTypeDto? travelExpenseType = await createTravelExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();
        ExpenseTypeDto? officeExpenseType = await createOfficeExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        // Create expense in travel expense type
        HttpResponseMessage createExpenseResponse = await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Equipment Purchase", travelExpenseType!.Id));
        ExpenseDto? createdExpense = await createExpenseResponse.Content.ReadFromJsonAsync<ExpenseDto>();

        // Update expense to office expense type
        var updateRequest = new UpdateExpenseCommand(createdExpense!.Id, "Equipment Purchase", officeExpenseType!.Id);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/expenses/{createdExpense.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ExpenseDto? result = await response.Content.ReadFromJsonAsync<ExpenseDto>();
        result!.ExpenseTypeId.Should().Be(officeExpenseType.Id);
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenExpenseDoesNotExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        string nonExistentId = "e_" + Faker.Random.Guid();
        var updateRequest = new UpdateExpenseCommand(nonExistentId, "Updated Name", expenseType!.Id);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/expenses/{nonExistentId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenExpenseTypeDoesNotExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type and expense
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        HttpResponseMessage createExpenseResponse = await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Hotel Booking", expenseType!.Id));
        ExpenseDto? createdExpense = await createExpenseResponse.Content.ReadFromJsonAsync<ExpenseDto>();

        // Try to update with non-existent expense type
        string nonExistentExpenseTypeId = "et_" + Faker.Random.Guid();
        var updateRequest = new UpdateExpenseCommand(createdExpense!.Id, "Updated Name", nonExistentExpenseTypeId);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/expenses/{createdExpense.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnConflict_WhenNameAlreadyExistsInTargetExpenseType()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        // Create two expenses
        await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Hotel Booking", expenseType!.Id));
        HttpResponseMessage createSecondExpenseResponse = await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Flight Tickets", expenseType.Id));
        ExpenseDto? secondExpense = await createSecondExpenseResponse.Content.ReadFromJsonAsync<ExpenseDto>();

        // Try to update second expense to have same name as first
        var updateRequest = new UpdateExpenseCommand(secondExpense!.Id, "Hotel Booking", expenseType.Id);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/expenses/{secondExpense.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Should_AllowSameNameForSameExpense()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type and expense
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        HttpResponseMessage createExpenseResponse = await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Hotel Booking", expenseType!.Id));
        ExpenseDto? createdExpense = await createExpenseResponse.Content.ReadFromJsonAsync<ExpenseDto>();

        // Update expense with same name
        var updateRequest = new UpdateExpenseCommand(createdExpense!.Id, "Hotel Booking", expenseType.Id);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/expenses/{createdExpense.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ExpenseDto? result = await response.Content.ReadFromJsonAsync<ExpenseDto>();
        result!.Name.Should().Be("Hotel Booking");
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenNameIsEmpty()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type and expense
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        HttpResponseMessage createExpenseResponse = await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Hotel Booking", expenseType!.Id));
        ExpenseDto? createdExpense = await createExpenseResponse.Content.ReadFromJsonAsync<ExpenseDto>();

        var updateRequest = new UpdateExpenseCommand(createdExpense!.Id, string.Empty, expenseType.Id);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/expenses/{createdExpense.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_AllowMovingExpenseWithSameNameToDifferentExpenseType()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create two expense types
        HttpResponseMessage createTravelExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        HttpResponseMessage createOfficeExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Office"));
        
        ExpenseTypeDto? travelExpenseType = await createTravelExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();
        ExpenseTypeDto? officeExpenseType = await createOfficeExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        // Create expense in travel expense type
        HttpResponseMessage createExpenseResponse = await client.PostAsJsonAsync("/api/budgets/expenses", new CreateExpenseCommand("Equipment Purchase", travelExpenseType!.Id));
        ExpenseDto? createdExpense = await createExpenseResponse.Content.ReadFromJsonAsync<ExpenseDto>();

        // Move expense to office expense type with same name (should be allowed)
        var updateRequest = new UpdateExpenseCommand(createdExpense!.Id, "Equipment Purchase", officeExpenseType!.Id);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/expenses/{createdExpense.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ExpenseDto? result = await response.Content.ReadFromJsonAsync<ExpenseDto>();
        result!.Name.Should().Be("Equipment Purchase");
        result.ExpenseTypeId.Should().Be(officeExpenseType.Id);
    }
}
