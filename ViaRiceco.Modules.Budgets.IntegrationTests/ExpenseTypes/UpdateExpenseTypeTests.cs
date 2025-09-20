using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.CreateExpenseType;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.UpdateExpenseType;
using ViaRiceco.Modules.Budgets.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.IntegrationTests.ExpenseTypes;

public sealed class UpdateExpenseTypeTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_UpdateExpenseType_WhenCommandIsValid()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        string originalName = "Travel";
        string updatedName = "Business Travel";

        // Create expense type first
        var createRequest = new CreateExpenseTypeCommand(originalName);
        HttpResponseMessage createResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", createRequest);
        ExpenseTypeDto? created = await createResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        var updateRequest = new UpdateExpenseTypeCommand(created!.Id, updatedName);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/expense-types/{created.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ExpenseTypeDto? result = await response.Content.ReadFromJsonAsync<ExpenseTypeDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be(updatedName);
        result.Id.Should().Be(created.Id);
        result.IsSystemDefined.Should().BeFalse();
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenExpenseTypeDoesNotExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        string nonExistentId = "et_" + Faker.Random.Guid();
        
        var updateRequest = new UpdateExpenseTypeCommand(nonExistentId, "Updated Name");

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/expense-types/{nonExistentId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnConflict_WhenNameAlreadyExistsForDifferentExpenseType()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create two expense types
        await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        HttpResponseMessage createResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Office Supplies"));
        ExpenseTypeDto? secondExpenseType = await createResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        // Try to update second expense type to have same name as first
        var updateRequest = new UpdateExpenseTypeCommand(secondExpenseType!.Id, "Travel");

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/expense-types/{secondExpenseType.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Should_AllowSameNameForSameExpenseType()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        string name = "Travel";

        HttpResponseMessage createResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand(name));
        ExpenseTypeDto? created = await createResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        var updateRequest = new UpdateExpenseTypeCommand(created!.Id, name);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/expense-types/{created.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ExpenseTypeDto? result = await response.Content.ReadFromJsonAsync<ExpenseTypeDto>();
        result!.Name.Should().Be(name);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenTryingToUpdateSystemDefinedExpenseType()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        // Use the known seeded expense type ID
        string systemExpenseTypeId = "et_db610449-8a5f-47d0-be6a-ec26e4945375";

        var updateRequest = new UpdateExpenseTypeCommand(systemExpenseTypeId, "Updated Investment");

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/expense-types/{systemExpenseTypeId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_HandleCaseChangesInName()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        string originalName = "travel expenses";
        string updatedName = "Travel Expenses";

        HttpResponseMessage createResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand(originalName));
        ExpenseTypeDto? created = await createResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        var updateRequest = new UpdateExpenseTypeCommand(created!.Id, updatedName);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/expense-types/{created.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ExpenseTypeDto? result = await response.Content.ReadFromJsonAsync<ExpenseTypeDto>();
        result!.Name.Should().Be(updatedName);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenNameIsEmpty()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        HttpResponseMessage createResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        ExpenseTypeDto? created = await createResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        var updateRequest = new UpdateExpenseTypeCommand(created!.Id, string.Empty);

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/budgets/expense-types/{created.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
