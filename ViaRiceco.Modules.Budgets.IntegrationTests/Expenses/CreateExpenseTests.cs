using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.CreateExpenseType;
using ViaRiceco.Modules.Budgets.Application.ExpenseTypes.Models;
using ViaRiceco.Modules.Budgets.Application.Expenses.CreateExpense;
using ViaRiceco.Modules.Budgets.Application.Expenses.Models;
using ViaRiceco.Modules.Budgets.IntegrationTests.Abstractions;

namespace ViaRiceco.Modules.Budgets.IntegrationTests.Expenses;

public sealed class CreateExpenseTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_CreateExpense_WhenCommandIsValid()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type first
        var createExpenseTypeRequest = new CreateExpenseTypeCommand("Travel");
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", createExpenseTypeRequest);
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        string expenseName = "Hotel Booking";
        var request = new CreateExpenseCommand(expenseName, expenseType!.Id);

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/budgets/expenses", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        ExpenseDto? result = await response.Content.ReadFromJsonAsync<ExpenseDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be(expenseName);
        result.ExpenseTypeId.Should().Be(expenseType.Id);
        result.Id.Should().StartWith("e_");

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/budgets/expenses/{result.Id}");
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenExpenseTypeDoesNotExist()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();
        string nonExistentExpenseTypeId = "et_" + Faker.Random.Guid();

        var request = new CreateExpenseCommand("Hotel Booking", nonExistentExpenseTypeId);

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/budgets/expenses", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Should_ReturnConflict_WhenExpenseNameAlreadyExistsInSameExpenseType()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type
        var createExpenseTypeRequest = new CreateExpenseTypeCommand("Travel");
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", createExpenseTypeRequest);
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        string expenseName = "Hotel Booking";
        var request = new CreateExpenseCommand(expenseName, expenseType!.Id);

        // Create first expense
        await client.PostAsJsonAsync("/api/budgets/expenses", request);

        // Act - Try to create duplicate expense in same expense type
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/budgets/expenses", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Should_AllowSameExpenseNameInDifferentExpenseTypes()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create two expense types
        HttpResponseMessage createTravelExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        HttpResponseMessage createOfficeExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Office"));
        
        ExpenseTypeDto? travelExpenseType = await createTravelExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();
        ExpenseTypeDto? officeExpenseType = await createOfficeExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        string expenseName = "Equipment Purchase";
        var travelRequest = new CreateExpenseCommand(expenseName, travelExpenseType!.Id);
        var officeRequest = new CreateExpenseCommand(expenseName, officeExpenseType!.Id);

        // Act
        HttpResponseMessage travelResponse = await client.PostAsJsonAsync("/api/budgets/expenses", travelRequest);
        HttpResponseMessage officeResponse = await client.PostAsJsonAsync("/api/budgets/expenses", officeRequest);

        // Assert
        travelResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        officeResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        ExpenseDto? travelExpense = await travelResponse.Content.ReadFromJsonAsync<ExpenseDto>();
        ExpenseDto? officeExpense = await officeResponse.Content.ReadFromJsonAsync<ExpenseDto>();

        travelExpense!.Name.Should().Be(expenseName);
        officeExpense!.Name.Should().Be(expenseName);
        travelExpense.ExpenseTypeId.Should().Be(travelExpenseType.Id);
        officeExpense.ExpenseTypeId.Should().Be(officeExpenseType.Id);
        travelExpense.Id.Should().NotBe(officeExpense.Id);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenNameIsEmpty()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        var request = new CreateExpenseCommand(string.Empty, expenseType!.Id);

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/budgets/expenses", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_HandleUnicodeCharacters_InExpenseName()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        string unicodeName = "Hôtel à Paris (パリのホテル) - فندق في باريس";
        var request = new CreateExpenseCommand(unicodeName, expenseType!.Id);

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/budgets/expenses", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        ExpenseDto? result = await response.Content.ReadFromJsonAsync<ExpenseDto>();
        result!.Name.Should().Be(unicodeName);
    }

    [Fact]
    public async Task Should_CreateMultipleExpensesInSameExpenseType_WithDifferentNames()
    {
        // Arrange
        await CleanDatabaseAsync();
        
        using HttpClient client = CreateJsonClient();

        // Create expense type
        HttpResponseMessage createExpenseTypeResponse = await client.PostAsJsonAsync("/api/budgets/expense-types", new CreateExpenseTypeCommand("Travel"));
        ExpenseTypeDto? expenseType = await createExpenseTypeResponse.Content.ReadFromJsonAsync<ExpenseTypeDto>();

        var firstRequest = new CreateExpenseCommand("Hotel Booking", expenseType!.Id);
        var secondRequest = new CreateExpenseCommand("Flight Tickets", expenseType.Id);

        // Act
        HttpResponseMessage firstResponse = await client.PostAsJsonAsync("/api/budgets/expenses", firstRequest);
        HttpResponseMessage secondResponse = await client.PostAsJsonAsync("/api/budgets/expenses", secondRequest);

        // Assert
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        ExpenseDto? firstExpense = await firstResponse.Content.ReadFromJsonAsync<ExpenseDto>();
        ExpenseDto? secondExpense = await secondResponse.Content.ReadFromJsonAsync<ExpenseDto>();

        firstExpense!.Name.Should().Be("Hotel Booking");
        secondExpense!.Name.Should().Be("Flight Tickets");
        firstExpense.Id.Should().NotBe(secondExpense.Id);
        firstExpense.ExpenseTypeId.Should().Be(expenseType.Id);
        secondExpense.ExpenseTypeId.Should().Be(expenseType.Id);
    }
}
