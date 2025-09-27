using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.CreateExpenseType;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models;
using ViaRiceco.Modules.Budgets.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.IntegrationTests.ExpenseTypes;

public sealed class GetExpenseTypeTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_ReturnExpenseType_WhenExpenseTypeExists()
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
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/expense-types/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ExpenseTypeDto? result = await response.Content.ReadFromJsonAsync<ExpenseTypeDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.Name.Should().Be(expenseTypeName);
        result.IsSystemDefined.Should().BeFalse();
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenExpenseTypeDoesNotExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        string nonExistentId = "et_" + Faker.Random.Guid();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/expense-types/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnSystemDefinedExpenseType()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        
        // Use the known seeded expense type ID
        string systemExpenseTypeId = "et_db610449-8a5f-47d0-be6a-ec26e4945375";

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/expense-types/{systemExpenseTypeId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ExpenseTypeDto? result = await response.Content.ReadFromJsonAsync<ExpenseTypeDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(systemExpenseTypeId);
        result.Name.Should().Be("Investment");
        result.IsSystemDefined.Should().BeTrue();
    }

    [Fact]
    public async Task Should_ReturnExpenseTypeWithCorrectFormat()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        string expenseTypeName = "Business Travel & Entertainment";

        var createRequest = new CreateExpenseTypeCommand(expenseTypeName);
        HttpResponseMessage createResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", createRequest);
        ExpenseTypeDto? created = await createResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/expense-types/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        ExpenseTypeDto? result = await response.Content.ReadFromJsonAsync<ExpenseTypeDto>();
        result.Should().NotBeNull();
        result!.Id.Should().StartWith("et_");
        result.Id.Should().HaveLength(39); // "et_" + GUID length
        result.Name.Should().Be(expenseTypeName);
    }

    [Theory]
    [InlineData("et_invalid-format")]
    [InlineData("invalid-id")]
    public async Task Should_ReturnNotFound_WhenIdFormatIsInvalid(string invalidId)
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Act
        HttpResponseMessage response = await client.GetAsync($"/api/budgets/expense-types/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
