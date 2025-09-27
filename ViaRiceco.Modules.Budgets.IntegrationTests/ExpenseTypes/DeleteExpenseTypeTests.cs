using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.CreateExpenseType;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models;
using ViaRiceco.Modules.Budgets.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.IntegrationTests.ExpenseTypes;

public sealed class DeleteExpenseTypeTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_DeleteExpenseType_WhenExpenseTypeExists()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        string expenseTypeName = "Travel";

        // Create expense type first
        var createRequest = new CreateExpenseTypeCommand(expenseTypeName);
        HttpResponseMessage createResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", createRequest);
        ExpenseTypeDto? created = await createResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/budgets/expense-types/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify expense type is deleted
        HttpResponseMessage getResponse = await client.GetAsync($"/api/budgets/expense-types/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenExpenseTypeDoesNotExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        string nonExistentId = "et_" + Faker.Random.Guid();

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/budgets/expense-types/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenTryingToDeleteSystemDefinedExpenseType()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        // For this test, we'll use the known seeded expense type ID
        string systemExpenseTypeId = "et_db610449-8a5f-47d0-be6a-ec26e4945375";

        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/budgets/expense-types/{systemExpenseTypeId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_ReturnConflict_WhenExpenseTypeHasAssociatedExpenses()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type
        var createExpenseTypeRequest = new CreateExpenseTypeCommand("Travel");
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", createExpenseTypeRequest);
        ExpenseTypeDto? createdExpenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        // Create expense associated with the expense type
        var createExpenseRequest = new
        {
            Name = "Hotel Booking",
            ExpenseTypeId = createdExpenseType!.Id
        };
        await client.PostAsJsonAsync("/api/budgets/expenses", createExpenseRequest);

        // Act - Try to delete expense type that has associated expenses
        HttpResponseMessage response = await client.DeleteAsync($"/api/budgets/expense-types/{createdExpenseType.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Should_DeleteMultipleExpenseTypes_Independently()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create multiple expense types
        HttpResponseMessage firstCreateResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        HttpResponseMessage secondCreateResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Office"));
        
        ExpenseTypeDto? firstExpenseType = await firstCreateResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();
        ExpenseTypeDto? secondExpenseType = await secondCreateResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        // Act - Delete first expense type
        HttpResponseMessage firstDeleteResponse = await client.DeleteAsync($"/api/budgets/expense-types/{firstExpenseType!.Id}");

        // Assert
        firstDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify first is deleted but second still exists
        HttpResponseMessage firstGetResponse = await client.GetAsync($"/api/budgets/expense-types/{firstExpenseType.Id}");
        HttpResponseMessage secondGetResponse = await client.GetAsync($"/api/budgets/expense-types/{secondExpenseType!.Id}");

        firstGetResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        secondGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
